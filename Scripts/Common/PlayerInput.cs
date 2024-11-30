
using UnityEngine;

public struct PlayerInputDatas {
    public float horizontal;
    public bool isAccelerating;
}
public class PlayerInput : MonoBehaviour
{
    public PlayerInputDatas CurrentInput {get ; private set;}
    public bool IsLocalPlayer  {get ; set;} = true;

    private void Update() {
        if(!IsLocalPlayer) return;

        CurrentInput = new PlayerInputDatas {
            horizontal = Input.GetAxis("Horizontal"),
            isAccelerating = true
        };
    }

}