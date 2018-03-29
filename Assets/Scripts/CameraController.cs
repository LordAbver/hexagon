using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float _rotationSpeed = 20f;
    private Quaternion _cameraRotation;
    private Vector3 _cameraMove;
    private Vector3 _centerRotation;
    private bool _isRotation;
    private Vector3 _mousePosition;
    private float _radius = 10f;
    private float _angleX;
    private float _angleY = 60f;

    private const Int16 MAX_CAMERA_ZOOM = 4;
    private const Int16 MIN_CAMERA_ZOOM = 20;

    private const Int16 MAX_CAMERA_ANGLE = 89;
    private const Int16 MIN_CAMERA_ANGLE = 30;

    private void Start()
	{
		//this.pgo = GameObject.Find("Characters");
	}

	private void LookAtCamera()
	{
		//for (int num = 0; num != this.pgo.transform.childCount; num++)
		//{
			//this.pgo.transform.GetChild(num).rotation = base.transform.rotation;
		//}
	}

    private void Update()
	{
        var speed = 100f;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
        }

        var camera = GetComponent<Camera>();
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (camera.fieldOfView > MAX_CAMERA_ZOOM)
            {
                camera.fieldOfView--;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (camera.fieldOfView < MIN_CAMERA_ZOOM)
            {
                camera.fieldOfView++;
            }
        }

        if (Input.GetMouseButtonDown(2))
        {
            RaycastHit hit3;
            if (Physics.Raycast(base.transform.position, base.transform.forward, out hit3, float.PositiveInfinity))
            {
                _isRotation = true;
                _centerRotation = hit3.point;
                _mousePosition = Input.mousePosition;
                _radius = Vector3.Distance(base.transform.position, _centerRotation);
            }
        }
        else if (Input.GetMouseButtonUp(2))
        {
            _isRotation = false;
        }
        if (_isRotation)
        {
            Vector3 mousePosition = Input.mousePosition;
            _angleX -= ((mousePosition.x - _mousePosition.x) * Time.deltaTime) * _rotationSpeed;
            _angleY -= ((mousePosition.y - _mousePosition.y) * Time.deltaTime) * _rotationSpeed;
            _mousePosition = mousePosition;

            //Adjust borders
            if (_angleY > MAX_CAMERA_ANGLE)
            {
                _angleY = MAX_CAMERA_ANGLE;
            }
            else if (_angleY < MIN_CAMERA_ANGLE)
            {
                _angleY = MIN_CAMERA_ANGLE;
            }

            if (_angleX > 360f)
            {
                _angleX -= 360f;
            }
            else if (_angleX < 0f)
            {
                _angleX += 360f;
            }
            float f = (3.141593f * _angleX) / 180f;
            float num4 = (3.141593f * _angleY) / 180f;
            float num5 = _radius * Mathf.Cos(num4);
            base.transform.position = new Vector3(_centerRotation.x + (num5 * Mathf.Sin(f)), _centerRotation.y + (_radius * Mathf.Sin(num4)), _centerRotation.z - (num5 * Mathf.Cos(f)));
            base.transform.LookAt(_centerRotation);
        }
    }
}
