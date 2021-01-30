using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Camera mainCam;
    // private PlayerStateMachine playercontrols;
    // [SerializeField] private PlayerInputManager inputManager;
    public float cameraFollowSpeed = 5f;
    public float targetBias = 0.5f;
    public float gamepadDist = 5f;
    Plane groundPlane;
    Vector3 s;
    // private void Awake()
    // {
    //     player = GameObject.FindGameObjectWithTag("Player").transform;
    // }
    // private void Start()
    // {
    //     playercontrols = player.gameObject.GetComponent<PlayerStateMachine>();
    //     transform.position = player.transform.position;
    // }

    // private void Update()
    // {
    //     GetCamPosition();

    //     s.y = player.transform.position.y;
    //     transform.position = Vector3.Lerp(transform.position, s, Time.deltaTime * cameraFollowSpeed);
    // }

    // void GetCamPosition()
    // {
    //     groundPlane = new Plane(Vector3.up, new Vector3(0, player.position.y, 0));
    //     Ray cameraRay = mainCam.ScreenPointToRay(inputManager.mouseLook);

    //     float rayLength;

    //     if (groundPlane.Raycast(cameraRay, out rayLength))
    //     {
    //         Vector3 rayPoint = cameraRay.GetPoint(rayLength);

    //         Vector3 dist = rayPoint - player.position;
    //         s = player.position + dist * targetBias;
    //     }
    //     else
    //     {
    //         s = player.position;
    //     }
    // }

    // void GamePadCam()
    // {
    //     Vector2 value = inputManager.controls.Gameplay.Rotate.ReadValue<Vector2>();
    //     Vector3 direction = new Vector3(value.x, 0, value.y);

    //     Vector3 horizMovement = playercontrols.right * direction.x;
    //     Vector3 vertikMovement = playercontrols.forward * direction.z;

    //     Vector3 moveD = horizMovement + vertikMovement;
    //     Vector3 f = player.position + moveD * gamepadDist;
    //     Vector3 dist = f - player.position;
    //     s = player.position + dist * targetBias;
    // }
}
