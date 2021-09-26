using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    InputMaster controls;


    public Transform cameraTarget;
    public Vector3 cameraOffset;
    public float rollSpeed = 3.0f;
    public ParticleSystem normalImpact;
    

    private bool isMoving = false;
    private bool isGrounded = true;


    // Start is called before the first frame update
    private void Start()
    {
        controls = InputManager.controls;
        MaterialManager.mainInstance.changeSideMaterial("Side1", MaterialManager.mainInstance.blue);
        
    }

    // Update is called once per frame
    void Update()
    {
        checkisGrounded();

        if (!isGrounded) return;
        if (isMoving) return;

        checkFaceDownSide();

        Vector3 direction = controls.Movement.Move.ReadValue<Vector2>();
        direction.z = direction.y;
        direction.y = 0;
        //direction = Vector3.ProjectOnPlane(cameraTarget.transform.TransformDirection(direction), Vector3.up);
        

        if (direction.sqrMagnitude != 0 && Physics.CheckBox(transform.position + (direction * 0.5f), new Vector3(0.45f, 0.25f, 0.9f), Quaternion.LookRotation(direction), 1 << 3))
        {
            //Debug.Log("WALL");
            return;
        }
        

        if (direction.sqrMagnitude == 1)
        {
            var pivot = transform.position + (Vector3.down + direction) * 0.5f;
            var axis = Vector3.Cross(Vector3.up, direction);
            StartCoroutine(Roll(pivot, axis));
        }

    }

    void checkisGrounded()
    {
        RaycastHit hit;
        int layerMask = (1 << 3) | (1 << 7);
        //Debug.DrawRay(transform.position, Vector3.down * 1);
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, layerMask);
        //Debug.Log("Grounded: " + isGrounded);
        if (isGrounded)
        {
            cameraTarget.position = new Vector3(hit.point.x, hit.point.y + 0.5f, hit.point.z) + cameraOffset;
        } else
        {
            cameraTarget.position = transform.position + cameraOffset;
        }

        if (!isGrounded)
        {
            GameStateManager.mainInstance.Lose();
        }
       
    }

    public void checkFaceDownSide() {

        RaycastHit hit;
        int layerMask = 1 << 6;
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z), Vector3.up * 10);
        string faceDownSideColor = "Gray";
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z), Vector3.up, out hit, 1.0f, layerMask))
        {
            faceDownSideColor = hit.collider.GetComponent<MeshRenderer>().material.name.Replace(" (Instance)", "");
           
        }


        layerMask = 1 << 7;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.0f, layerMask))
        {
            if (hit.collider.tag.Equals("Hazard"))
            {
                GameStateManager.mainInstance.Lose();
            }
            if (hit.collider.tag.Contains("Hazard:"))
            {
                string[] hazardColors = hit.collider.tag.Replace("Hazard:", "").Split(',');
                for (int i = 0; i < hazardColors.Length; i++)
                {
                    if(faceDownSideColor.Equals(hazardColors[i]))
                    {
                        GameStateManager.mainInstance.Lose();
                    }
                }
                
            }

        }


    }


    IEnumerator Roll(Vector3 pivot, Vector3 axis)
    {
        isMoving = true;
        
        for (int i = 0; i < 90 / rollSpeed; i++)
        {
            transform.RotateAround(pivot, axis, rollSpeed);
            yield return new WaitForSeconds(0.01f);
        }

        float xSnap = Mathf.Round(transform.position.x * 2) / 2;
        float ySnap = Mathf.Round(transform.position.y * 2) / 2;
        float zSnap = Mathf.Round(transform.position.z * 2) / 2;
        transform.position = new Vector3(xSnap, ySnap, zSnap);
        cameraTarget.position = transform.position + cameraOffset;

        normalImpact.transform.position = new Vector3(cameraTarget.position.x, cameraTarget.position.y - 0.5f, cameraTarget.position.z);
        normalImpact.Play();

        isMoving = false;
    }

    Vector3 SnapTo(Vector3 v3, float snapAngle)
    {
        float angle = Vector3.Angle(v3, Vector3.up);
        if (angle < snapAngle / 2.0f)          // Cannot do cross product 
            return Vector3.up * v3.magnitude;  //   with angles 0 & 180
        if (angle > 180.0f - snapAngle / 2.0f)
            return Vector3.down * v3.magnitude;

        float t = Mathf.Round(angle / snapAngle);
        float deltaAngle = (t * snapAngle) - angle;

        Vector3 axis = Vector3.Cross(Vector3.up, v3);
        Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
        return q * v3;
    }


}
