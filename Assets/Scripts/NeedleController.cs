using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleController : MonoBehaviour {

	[SerializeField]
	private KeyCode upKey;
	[SerializeField]
	private KeyCode downKey;
	[SerializeField]
	private KeyCode leftKey;
	[SerializeField]
	private KeyCode rightKey;

	private Coroutine moveCoroutine;

	private float moveSpeed = 0.5f;
	private float maxUpDistance = 5f;
	private float maxDownDistance = 5f;
	private float rotationSpeed = 10f;
	float maxRotation = 45f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {
		if (this.moveCoroutine != null)
		{
			return;
		}

		if (Input.GetKey(this.upKey))
		{
			this.moveCoroutine = StartCoroutine(this.MoveVertical(this.upKey));
		}
		else if (Input.GetKey(this.downKey))
		{
			this.moveCoroutine = StartCoroutine(this.MoveVertical(this.downKey));
		}
		else if (Input.GetKey(this.leftKey))
		{
			this.moveCoroutine = StartCoroutine(this.MoveHorizontal(this.leftKey));
		}
		else if (Input.GetKey(this.rightKey))
		{
			this.moveCoroutine = StartCoroutine(this.MoveHorizontal(this.rightKey));
		}
	}

	private IEnumerator MoveVertical(KeyCode pressedKey)
	{
		Vector3 originalPosition = this.transform.position;
		Vector3 cachedPosition = this.transform.position;
		Vector3 finalPosition = cachedPosition;

		while (Input.GetKey(pressedKey))
		{
			if (pressedKey == this.upKey)
			{
				finalPosition = new Vector3(originalPosition.x, originalPosition.y + this.maxUpDistance, originalPosition.z);
			}
			else if (pressedKey == this.downKey)
			{
				finalPosition = new Vector3(originalPosition.x, originalPosition.y - this.maxDownDistance, originalPosition.z);
			}

			cachedPosition = Vector3.Lerp(cachedPosition, finalPosition, this.moveSpeed);
			this.transform.position = cachedPosition;
			yield return null;
		}

		while (cachedPosition != originalPosition)
		{
			if (Vector3.Magnitude(cachedPosition - originalPosition) < 0.01f)
			{
				cachedPosition = originalPosition;
			}
			else
			{
				cachedPosition = Vector3.Lerp(cachedPosition, originalPosition, this.moveSpeed);
			}

			this.transform.position = cachedPosition;
		}

		this.moveCoroutine = null;
	}

	private IEnumerator MoveHorizontal(KeyCode pressedKey)
	{
		Transform originalTransform = this.transform;

		Transform cachedTransform = this.transform;

		float finalRotation = (pressedKey == this.leftKey) ? this.maxRotation : 360 - this.maxRotation;

		float thisRotationSpeed = (pressedKey == this.leftKey) ? this.rotationSpeed : -this.rotationSpeed; 

		while (Input.GetKey(pressedKey))
		{
			if (cachedTransform.eulerAngles.z != finalRotation)
			{
				cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, cachedTransform.eulerAngles.z + thisRotationSpeed);

				if (pressedKey == this.leftKey && Mathf.Abs(cachedTransform.eulerAngles.z) > finalRotation)
				{
					cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, finalRotation);
				}
				else if (pressedKey == this.rightKey && Mathf.Abs(cachedTransform.eulerAngles.z) < finalRotation)
				{
					cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, finalRotation);
				}

				this.transform.rotation = cachedTransform.rotation;
			}

			yield return null;
		}

		while (cachedTransform.eulerAngles.z != 0)
		{
			cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, cachedTransform.eulerAngles.z - thisRotationSpeed);

			if (pressedKey == this.leftKey && cachedTransform.eulerAngles.z > 300)
			{
				cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, 0);
			}
			else if (pressedKey == this.rightKey && cachedTransform.eulerAngles.z < finalRotation)
			{
				cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, 0);
			}

			this.transform.rotation = cachedTransform.rotation;
			yield return null;
		}

		this.moveCoroutine = null;
	}
}
