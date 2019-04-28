using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleController : MonoBehaviour {

	private enum NeedleSide { Left, Right };

	[SerializeField]
	private NeedleSide side;

	[SerializeField]
	private KeyCode upKey;
	[SerializeField]
	private KeyCode downKey;
	[SerializeField]
	private KeyCode leftKey;
	[SerializeField]
	private KeyCode rightKey;

	[SerializeField]
	private SynapseLocation leftSynapse;
	[SerializeField]
	private SynapseLocation rightSynapse;
	[SerializeField]
	private SynapseLocation upSynapse;
	[SerializeField]
	private SynapseLocation downSynapse;

	private Coroutine moveCoroutine;

	private float moveSpeed = 0.5f;
	private float maxUpDistance = 1f;
	private float maxDownDistance = 3f;
	private float rotationSpeed = 10f;
	private float maxRotation = 39f;

	public delegate void SynapseHit(SynapseLocation hitSynapse);
	public static event SynapseHit onSynapseHit;

  public bool isTutorialAnimating = false;

	// Update is called once per frame
	void Update() {
    if (this.isTutorialAnimating == true)
    {
      return;
    }

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

		bool synapseHit = false;

		while (Input.GetKey(pressedKey))
		{
			if (pressedKey == this.upKey)
			{
				finalPosition = new Vector3(originalPosition.x, originalPosition.y + this.maxUpDistance, originalPosition.z);
				if (synapseHit == false)
				{
					NeedleController.onSynapseHit(this.upSynapse);
					synapseHit = true;
				}
			}
			else if (pressedKey == this.downKey)
			{
				finalPosition = new Vector3(originalPosition.x, originalPosition.y - this.maxDownDistance, originalPosition.z);
				if (synapseHit == false)
				{
					NeedleController.onSynapseHit(this.downSynapse);
					synapseHit = true;
				}
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

		if (this.side == NeedleSide.Left && pressedKey == this.rightKey)
		{
			finalRotation += ((360f - finalRotation) / 2.5f);
		}
		else if (this.side == NeedleSide.Right && pressedKey == this.leftKey)
		{
			finalRotation = finalRotation - (finalRotation / 2.5f);
		}

		bool synapseHit = false;

		while (Input.GetKey(pressedKey))
		{
			if (cachedTransform.eulerAngles.z != finalRotation)
			{
				cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, cachedTransform.eulerAngles.z + thisRotationSpeed);

				if (pressedKey == this.leftKey && Mathf.Abs(cachedTransform.eulerAngles.z) > finalRotation)
				{
					cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, finalRotation);
					if (synapseHit == false)
					{
						NeedleController.onSynapseHit(this.leftSynapse);
						synapseHit = true;
					}
					
				}
				else if (pressedKey == this.rightKey && Mathf.Abs(cachedTransform.eulerAngles.z) < finalRotation)
				{
					cachedTransform.eulerAngles = new Vector3(cachedTransform.eulerAngles.x, cachedTransform.eulerAngles.y, finalRotation);
					if (synapseHit == false)
					{
						NeedleController.onSynapseHit(this.rightSynapse);
						synapseHit = true;
					}
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
