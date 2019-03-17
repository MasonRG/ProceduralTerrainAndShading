using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain
{
	public class CameraController : MonoBehaviour
	{
		const float THRESHOLD = 0.10f;

		public float moveSpeed = 4f;
		public float lookSpeed = 9f;

		[Range(0, 1)]
		public float moveDamping = 0.1f;

		public KeyCode fastKey = KeyCode.LeftShift;
		public KeyCode slowKey = KeyCode.LeftControl;

		Vector3 move = new Vector3();


		private void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			RotateCamera();
			MoveCamera();
		}

		private void OnApplicationFocus(bool focus)
		{
			if (focus)
				Cursor.lockState = CursorLockMode.Locked;
		}

		void MoveCamera()
		{
			float spd = moveSpeed * 10f * Time.deltaTime;
			if (Input.GetKey(fastKey)) spd *= 2.5f;
			else if (Input.GetKey(slowKey)) spd *= 0.5f;

			float fac = 1 + (Time.deltaTime / Time.fixedDeltaTime);

			float Z = Input.GetAxisRaw("Front Back");
			float X = Input.GetAxisRaw("Left Right");
			float Y = Input.GetAxisRaw("Up Down");
			move.z = Mathf.Lerp(move.z, Z, moveDamping * fac);
			move.x = Mathf.Lerp(move.x, X, moveDamping * fac);
			move.y = Mathf.Lerp(move.y, Y, moveDamping * fac);
			if (Mathf.Abs(move.z) <= THRESHOLD) move.z = Z;
			if (Mathf.Abs(move.x) <= THRESHOLD) move.x = X;
			if (Mathf.Abs(move.y) <= THRESHOLD) move.y = Y;

			var displacement = (transform.forward * move.z) + (transform.right * move.x) + (Vector3.up * move.y);
			transform.position += displacement * spd;
		}


		void RotateCamera()
		{
			const float BOUND = 80f;

			Vector3 euler = transform.eulerAngles;

			float scl = lookSpeed * 10f;
			float mouseX = Input.GetAxisRaw("Mouse X");
			float mouseY = Input.GetAxisRaw("Mouse Y");

			float speedX = scl * Time.deltaTime;
			float speedY = speedX;
			float yaw = speedX * mouseX;
			float pitch = speedY * mouseY;

			euler.x -= pitch;
			euler.y += yaw;
			euler.z = 0f;

			//Ensure x doesnt over-rotate
			//0 is middle -> goes 'down' to 90
			//360 also is middle -> goes 'up' to 270
			if (euler.x > BOUND && euler.x <= 180f)
				euler.x = BOUND;
			else if (euler.x < 360f - BOUND && euler.x > 180f)
				euler.x = 360f - BOUND;

			transform.eulerAngles = euler;
		}
	}
}