using UnityEngine;

public class Sway : MonoBehaviour
{
    [Header("Sway")]
     public float smooth;
     public float multiplier;

     private void Update()
     {
        //input do rato
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion tragetRotation = rotationX * rotationY;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, tragetRotation, smooth * Time.deltaTime);
     }
    
}
