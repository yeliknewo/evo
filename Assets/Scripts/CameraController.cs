using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {


	//usually not null
	Camera GetCamera()
	{
		return GetComponent<Camera>();
	}
}
