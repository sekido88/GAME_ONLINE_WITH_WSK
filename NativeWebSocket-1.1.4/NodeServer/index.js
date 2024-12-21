const crypto = require("crypto");
const express = require("express");
const { createServer } = require("http");
const WebSocket = require("ws");
const uuid = require("uuid-random");

const app = express();
const port = 8080;

const server = createServer(app);
const wss = new WebSocket.Server({ server });

exports.WS_Export = wss;


const rooms = new Map();
const clients = new Map();
const players = new Map();

wss.on("connection", function (ws) {

  const clientId = uuid();
  ws.id = clientId;

  clients.set(clientId, ws);
  players.set(clientId, {
    ws: ws,
    name: null,
    roomId: null,
    isReady: false,
    position: null,
    rotation: null,
  });

  console.log(`Client connected: ${clientId}`);

  wss.on("connection", function (ws) {
    console.log("New client connected");

    const clientId = uuid();
    ws.id = clientId;

    clients.set(clientId, ws);
   
    console.log(`Assigned client ID: ${clientId}`);

    sendToClient(ws, {
      action: "connected",
      playerId: clientId,
    });

    ws.on("message", function (data) {
      try {
        const message = JSON.parse(data);
        console.log("Received message from client:", message);

        handleMessage(ws, message);
      } catch (error) {
        console.error("Error handling message:", error);
      }
    });
  });

  ws.on("close", function () {
    handlePlayerDisconnect(ws);
    clients.delete(clientId);
    players.delete(clientId);
    console.log(`Client disconnected: ${clientId}`);
  });

});

function handleMessage(ws, message) {
  switch (message.action) {
    case "create_room":
      handleCreateRoom(ws, message);
      break;
    case "join_room":
      handleJoinRoom(ws, message);
      break;
    case "player_moved":
      handlePlayerMove(ws, message);
      break;
    case "player_ready":
      handlePlayerReady(ws, message);
      break;
    case "start_race":
      handleStartRace(ws, message);
      break;
    case "race_started":
      handleRaceStarted(ws, message);
      break;
    case "player_checkpoint":
      handlePlayerCheckpoint(ws, message);
      break;
    case "race_finished":
      handleRaceFinished(ws, message);
      break;
  }
}

function handleCreateRoom(ws, message) {
  const roomId = generateRoomId();
  console.log(`Creating room: ${roomId} for client: ${ws.id}`);

  const room = {
    id: roomId,
    host: ws.id,
    players: new Map(),
    isRacing: false,
  };

  rooms.set(roomId, room);

  room.players.set(ws.id, {
    ws: ws,
    name: message.playerName,
    playerId: ws.id,
    roomId: roomId,
    isReady: false,
    position: null,
    rotation: null,
  });

  ws.roomId = roomId;

  sendToClient(ws, {
    action: "room_created",
    roomId: roomId,
    players: getPlayersData(room),
    playerId: ws.id,
    isHost: true,
  });
}

function handleJoinRoom(ws, message) {
  const room = rooms.get(message.roomId);
  if (!room) {
    sendToClient(ws, {
      action: "error",
      error: "Room not found",
    });
    return;
  }

  room.players.set(ws.id, {
    ws: ws,
    name: message.playerName,
    playerId: ws.id,
    roomId: message.roomId,
    isReady: false,
    position: null,
    rotation: null,
  });



  ws.roomId = message.roomId;

  broadcastToRoom(room, {
    action: "player_joined",
    players: getPlayersData(room),
    playerId: ws.id,
  });

  sendToClient(ws, {
    action: "room_joined",
    roomId: message.roomId,
    playerId: ws.id,
    isHost: false,
    players: getPlayersData(room),
  });
}

function handlePlayerMove(ws, message) {
  const room = rooms.get(ws.roomId);
  if (!room) return;

  broadcastToRoom(
    room,
    {
      action: "player_moved",
      playerId: ws.id,
      position: message.position,
      rotation: message.rotation,
    },
    ws.id
  );
}

function handlePlayerDisconnect(ws) {
  const room = rooms.get(ws.roomId);
  if (!room) return;

  room.players.delete(ws.id);

  if (room.players.size === 0) {
    rooms.delete(ws.roomId);
  } else {
    broadcastToRoom(room, {
      action: "player_left",
      playerId: ws.id,
    });
  }
}

function generateRoomId() {
  return Math.random().toString(36).substring(2, 8).toUpperCase();
}

function sendToClient(ws, message) {
  try {
    console.log("Sending to client:", message);
    ws.send(JSON.stringify(message));
  } catch (error) {
    console.error("Error sending message:", error);
  }
}

function handlePlayerReady(ws, message) {
  const room = rooms.get(ws.roomId);
  if (!room) return;

  const player = room.players.get(ws.id);
  if (player) {
    player.isReady = message.isReady;

    broadcastToRoom(room, {
      action: "player_ready",
      players: getPlayersData(room),
    });

  }
}

function broadcastToRoom(room, message, excludeClientId = null) {
  room.players.forEach((player, playerId) => {
    if (playerId !== excludeClientId) {
      sendToClient(player.ws, message);
    }
  });
}

function handleStartRace(ws, message) {
  const room = rooms.get(ws.roomId);
  if (!room || room.host !== ws.id) return;

  const allReady = Array.from(room.players.values()).every((p) => p.isReady);
  if (!allReady) return;

  broadcastToRoom(room, {
    action: "game_starting",
    players: getPlayersData(room),
  });

  setTimeout(() => {
    room.isRacing = true;
    broadcastToRoom(room, {
      action: "game_started",
      players: getPlayersData(room),
    });
  }, 3000);
}

function getPlayersData(room) {
  return Array.from(room.players.entries()).map(([id, player]) => ({
    id: id,
    name: player.name,
    isReady: player.isReady,
    spawnPosition: calculateSpawnPosition(room.players.size),
  }));
}

function calculateSpawnPosition(playerIndex) {
  // Tính toán vị trí spawn cho mỗi player
  const spacing = 2; // Khoảng cách giữa các player
  return {
    x: playerIndex * spacing,
    y: 0,
    z: 0,
  };
}

server.listen(port, function () {
  console.log(`Listening on http://localhost:${port}`);
});
