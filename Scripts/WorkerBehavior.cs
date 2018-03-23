﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkerBehavior : MonoBehaviour {

	public Vector3 pickUpLocation;
	public Vector3 dropOffLocation;
	public float moveTime = .5f;
	public double loadSpeedPerSecond = 25;
	public double carryCapacity = 100;
	public double currentAmountCarried = 0;
	public BucketBehavior myPickUpBucketScript;
	public BucketBehavior myDropOffBucketScript;
	public Text AmountDisplay;

	private Rigidbody2D rb2D;
	private float inverseMoveTime;
	private float inverseLoadTime;
	private bool NextWillPickUp = true;

	void Awake () {
		rb2D = GetComponent <Rigidbody2D> ();
		inverseMoveTime = 1f / moveTime;

		setPickUpAndDropOffLocations(gameObject);
		Move (pickUpLocation);
	}

	protected void Move (Vector2 destination)
	{
		StartCoroutine (SmoothMovement (destination));
	}

	private void CallPickUpOrDropOff ()
	{
		if (NextWillPickUp)
			myPickUpBucketScript.InitiatePickUpResources (gameObject);
		else
			myDropOffBucketScript.InitiateDropOffResources (gameObject);
	}

	public void TransferAmount(double amount)
	{
		float currentLoadTime = (float)(amount / loadSpeedPerSecond);
		AnimateLoadingOrUnloading (currentLoadTime);
		if (NextWillPickUp)
			StartCoroutine(PickUpAndTurnBack(amount, currentLoadTime));
		else
			StartCoroutine(DropOffAndTurnBack(amount, currentLoadTime));
	}
		
	protected IEnumerator PickUpAndTurnBack (double amount, float currentLoadTime) {
		yield return new WaitForSeconds (currentLoadTime);
		if (gameObject.tag == "Miner")
			currentAmountCarried = carryCapacity;
		else if (amount < myPickUpBucketScript.currentBucketAmount) {
			currentAmountCarried += amount;
			myPickUpBucketScript.UpdateBucketResources (-amount);
		} else { 
			currentAmountCarried = myPickUpBucketScript.currentBucketAmount;
			myPickUpBucketScript.UpdateBucketResources (-myPickUpBucketScript.currentBucketAmount);
		}
		TurnAroundAndStartWalkingBack ();
	}

	protected IEnumerator DropOffAndTurnBack (double amount, float currentLoadTime) {
		if (gameObject.tag == "Miner")
			currentLoadTime = 0;
		yield return new WaitForSeconds (currentLoadTime);
		currentAmountCarried = 0;
		myDropOffBucketScript.UpdateBucketResources (amount);
		TurnAroundAndStartWalkingBack ();
	}

	private void AnimateLoadingOrUnloading(float loadTime)
	{
		
	}
		
	public void TurnAroundAndStartWalkingBack() 
	{
		NextWillPickUp = !NextWillPickUp;
		Vector2 nextDestination = NextWillPickUp ? pickUpLocation : dropOffLocation;
		gameObject.GetComponent<SpriteRenderer> ().flipX = !gameObject.GetComponent<SpriteRenderer> ().flipX;
		setCarryAmountToDisplay ();
		Move (nextDestination);
	}


	protected IEnumerator SmoothMovement (Vector3 destination)
	{
		float sqrRemainingDistance = (transform.position - destination).sqrMagnitude;
		while (sqrRemainingDistance > .01f) 
		{
			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, destination, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - destination).sqrMagnitude;
			yield return null;
		}
		CallPickUpOrDropOff ();
	}


	private void setPickUpAndDropOffLocations(GameObject currentObject)
	{
		if (gameObject.tag == "Transporter") {
			pickUpLocation = new Vector2 (-3.7f, 0.76f);
			dropOffLocation = new Vector2 (4.17f, 0.76f);
			myPickUpBucketScript = GameObject.FindGameObjectWithTag ("ElevatorDeposit").GetComponent<BucketBehavior>();
			myDropOffBucketScript = GameObject.FindGameObjectWithTag ("GameManagerDeposit").GetComponent<BucketBehavior>();
		} else if (gameObject.tag == "Miner") {
			pickUpLocation = new Vector2 (transform.parent.localPosition.x + 1.7f, transform.parent.localPosition.y);
			dropOffLocation = new Vector2 (transform.parent.localPosition.x - 2, transform.parent.localPosition.y);
			myPickUpBucketScript = transform.parent.Find("RawMaterials").GetComponent<BucketBehavior>();
			myDropOffBucketScript = transform.parent.Find("MineDeposit").GetComponent<BucketBehavior>();
		} else if (gameObject.tag == "Elevator") {
			pickUpLocation = new Vector2 (-5.35f, -4.5f);
			dropOffLocation = new Vector2 (-5.35f, -1.77f);
			myPickUpBucketScript = GameObject.Find ("MineShaft").transform.GetChild(0).GetComponent<BucketBehavior>();
			myDropOffBucketScript = GameObject.FindGameObjectWithTag ("ElevatorDeposit").GetComponent<BucketBehavior>();
			if (gameObject.tag == "Elevator")
				print (myPickUpBucketScript);
		}
	}

	private void setCarryAmountToDisplay () {
		if (gameObject.tag == "Miner")
			return;
		if (currentAmountCarried == 0)
			AmountDisplay.text = "";
		else
			AmountDisplay.text = currentAmountCarried.ToString ("C0");
	}
}