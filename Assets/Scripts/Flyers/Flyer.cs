using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class Flyer : MonoBehaviour
{
    // [SerializeField] private float idleSpeed, turnSpeed, switchSeconds, idleRatio, delayStart, randomBaseOffset = 5;
    //
    // [SerializeField] private Vector2 animSpeedMinMax, moveSpeedMinMax, changeAnimEveryFromTo, changeTargetEveryFromTo, 
    //     radiusMinMax, yMinMax;
    //
    // [SerializeField] private Transform homeTarget, flyingTarget;
    // [SerializeField] private bool returnToBase;
    //
    // private Animator animator;
    // private Rigidbody rigidbody;
    //
    // private float changeTarget,
    //     changeAnim,
    //     timeSinceTarget,
    //     timeSinceAnim,
    //     prevAnim,
    //     currentAnim,
    //     speed,
    //     prevSpeed,
    //     zTurn,
    //     prevZ,
    //     turnSpeedBackup,
    //     distanceFromBase,
    //     distanceFromTarget;
    //
    // private Vector3 rotateTarget, position, direction, velocity, randomizedBase;
    // private Quaternion lookRotation;
    //
    // private void Start()
    // {
    //     animator = GetComponent<Animator>();
    //     rigidbody = GetComponent<Rigidbody>();
    //     turnSpeedBackup = turnSpeed;
    //     direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
    //     if (delayStart < 0) rigidbody.velocity = idleSpeed * direction;
    // }
    //
    // private void FixedUpdate()
    // {
    //     // Wait if start should be delayed
    //     if (delayStart > 0)
    //     {
    //         delayStart -= Time.fixedDeltaTime;
    //         return;
    //     }
    //     
    //     // Calculate distances
    //     distanceFromBase = Vector3.Magnitude(randomizedBase - rigidbody.position);
    //     distanceFromTarget = Vector3.Magnitude(flyingTarget.position - rigidbody.position);
    //     
    //     // Time for a new animation speed
    //     if (changeAnim < 0)
    //     {
    //         prevAnim = currentAnim;
    //         currentAnim = ChangeAnim(currentAnim);
    //         changeAnim = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
    //         timeSinceAnim = 0;
    //         prevSpeed = speed;
    //         if (currentAnim == 0) speed = idleSpeed;
    //         else
    //             speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y,
    //                 (currentAnim - animSpeedMinMax.x) / (animSpeedMinMax.y - animSpeedMinMax.x));
    //     }
    //     
    //     // Time for a new target position
    //     if (changeTarget < 0)
    //     {
    //         rotateTarget = ChangeDirection(rigidbody.position);
    //         if (returnToBase) changeTarget = 0.2f;
    //         else changeTarget = Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
    //         timeSinceTarget = 0;
    //     }
    //     
    //     // Update timers
    //     changeAnim -= Time.fixedDeltaTime;
    //     changeTarget -= Time.fixedDeltaTime;
    //     timeSinceTarget += Time.fixedDeltaTime;
    //     timeSinceAnim += Time.fixedDeltaTime;
    //     
    //     // Rotate towards target
    //     if (rotateTarget != Vector3.zero) lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
    //     Vector3 rotation = Quaternion.RotateTowards(rigidbody.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime)
    //         .eulerAngles;
    //     rigidbody.transform.eulerAngles = rotation;
    //     
    //     // Move flyer
    //     direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
    //     rigidbody.velocity = Mathf.Lerp(prevSpeed, speed, Mathf.Clamp(timeSinceAnim / switchSeconds, 0, 1)) * direction;
    // }
    //
    // // Randomly select a new direction to fly to
    // private Vector3 ChangeDirection(Vector3 rigidbodyPosition)
    // {
    //     Vector3 newDir;
    //     
    //     // 360 degrees of choice on the horizontal plane
    //     float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
    //     
    //     // Limited max stepness of ascent/descent in the vertical plane
    //     float angleY = Random.Range(-Mathf.PI / 50, Mathf.PI / 50);
    //     
    //     // Calculate direction
    //     newDir = Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right +
    //              Mathf.Sin(angleY) * Vector3.up;
    //     return newDir;
    // }
    //
    // private float ChangeAnim(float f)
    // {
    //     float newState;
    //     if (Random.Range(0, 1) < idleRatio) newState = 0;
    //     else newState = Random.Range(animSpeedMinMax.x, animSpeedMinMax.y);
    //
    //     if (newState != currentAnim)
    //     {
    //         animator.SetFloat("FlySpeed", newState);
    //         if (newState == 0) animator.speed = 1;
    //         else animator.speed = newState;
    //     }
    //     
    //     return newState;
    // }

    [SerializeField] private float idleSpeed, turnSpeed, switchSeconds, idleRatio;
    [SerializeField] private Vector2 animSpeedMinMax, moveSpeedMinMax, changeAnimEveryFromTo, changeTargetEveryFromTo;
    [SerializeField] private Transform homeTarget, flyingTarget;
    [SerializeField] private Vector2 radiusMinMax;
    [SerializeField] private Vector2 yMinMax;
    [SerializeField] private bool returnToBase;
    [SerializeField] private float randomBaseOffset = 5, delayStart;

    private Animator animator;
    private new Rigidbody rigidbody;
    private float changeTarget, changeAnim, timeSinceTarget, timeSinceAnim, prevAnim, currentAnim, prevSpeed, speed, zTurn, prevZ, turnSpeedBackup;
    private Vector3 rotateTarget, position, direction, velocity, randomizedBase;
    private Quaternion lookRotation;
    private float distanceFromBase, distanceFromTarget;


    private void Start()
    {
        // Inititalize
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        turnSpeedBackup = turnSpeed;
        direction = Quaternion.Euler(transform.eulerAngles) * (Vector3.forward);
        if (delayStart < 0f) rigidbody.velocity = idleSpeed * direction;
    }

    private void FixedUpdate()
    {
        // Wait if start should be delayed (useful to add small differences in large flocks)
        if (delayStart > 0f)
        {
            delayStart -= Time.fixedDeltaTime;
            return;
        }
        
        // Calculate distances
        distanceFromBase = Vector3.Magnitude(randomizedBase - rigidbody.position);
        distanceFromTarget = Vector3.Magnitude(flyingTarget.position - rigidbody.position);
        
        // Allow drastic turns close to base to ensure target can be reached
        if (returnToBase && distanceFromBase < 10f)
        {
            if (turnSpeed != 300f && rigidbody.velocity.magnitude != 0f)
            {
                turnSpeedBackup = turnSpeed;
                turnSpeed = 300f;
            } else if (distanceFromBase <= 2f)
            {
                rigidbody.velocity = Vector3.zero;
                turnSpeed = turnSpeedBackup;
                return;
            }
        }
        
        // Time for a new animation speed
        if (changeAnim < 0f)
        {
            prevAnim = currentAnim;
            currentAnim = ChangeAnim(currentAnim);
            changeAnim = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
            timeSinceAnim = 0f;
            prevSpeed = speed;
            if (currentAnim == 0) speed = idleSpeed;
            else speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, (currentAnim - animSpeedMinMax.x) / (animSpeedMinMax.y - animSpeedMinMax.x));
        }
        
        // Time for a new target position
        if (changeTarget < 0f)
        {
            rotateTarget = ChangeDirection(rigidbody.transform.position);
            if (returnToBase) changeTarget = 0.2f; else changeTarget = Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
            timeSinceTarget = 0f;
        }
        
        // Turn when approaching height limits
        // TODO: Adjust limit and "exit direction" by object's direction and velocity, instead of the 10f and 1f - this works in my current scenario/scale
        if (rigidbody.transform.position.y < yMinMax.x + 10f ||
            rigidbody.transform.position.y > yMinMax.y -10f)
        {
            if (rigidbody.transform.position.y < yMinMax.x + 10f) rotateTarget.y = 1f; else rotateTarget.y = -1f;
        }
        rigidbody.transform.Rotate(0f, 0f, -prevZ, Space.Self);
        zTurn = Mathf.Clamp(Vector3.SignedAngle(rotateTarget, direction, Vector3.up), -45f, 45f);
        
        // Update times
        changeAnim -= Time.fixedDeltaTime;
        changeTarget -= Time.fixedDeltaTime;
        timeSinceTarget += Time.fixedDeltaTime;
        timeSinceAnim += Time.fixedDeltaTime;

        // Rotate towards target
        if (rotateTarget != Vector3.zero) lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
        Vector3 rotation = Quaternion.RotateTowards(rigidbody.transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        rigidbody.transform.eulerAngles = rotation;
        
        // Rotate on z-axis to tilt body towards turn direction
        float temp = prevZ;
        if (prevZ < zTurn) prevZ += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zTurn - prevZ);
        else if (prevZ >= zTurn) prevZ -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevZ - zTurn);
        
        // Min and max rotation on z-axis - can also be parameterized
        prevZ = Mathf.Clamp(prevZ, -45f, 45f);
        
        // Remove temp if transform is rotated back earlier in FixedUpdate
        rigidbody.transform.Rotate(0f, 0f, prevZ - temp, Space.Self);
        
        // Move flyer
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        if (returnToBase && distanceFromBase < idleSpeed)
        {
            rigidbody.velocity = Mathf.Min(idleSpeed, distanceFromBase) * direction;
        } else rigidbody.velocity = Mathf.Lerp(prevSpeed, speed, Mathf.Clamp(timeSinceAnim / switchSeconds, 0f, 1f)) * direction;
        
        // Hard-limit the height, in case the limit is breached despite of the turnaround attempt
        if (rigidbody.transform.position.y < yMinMax.x || rigidbody.transform.position.y > yMinMax.y)
        {
            position = rigidbody.transform.position;
            position.y = Mathf.Clamp(position.y, yMinMax.x, yMinMax.y);
            rigidbody.transform.position = position;
        }
    }

    // Select a new animation speed randomly
    private float ChangeAnim(float currentAnim)
    {
        float newState;
        if (Random.Range(0f, 1f) < idleRatio) newState = 0f;
        else newState = Random.Range(animSpeedMinMax.x, animSpeedMinMax.y);
        
        if (newState != currentAnim)
        {
            animator.SetFloat("FlySpeed", newState);
            if (newState == 0) animator.speed = 1f; 
            else animator.speed = newState;
        }
        return newState;
    }

    // Select a new direction to fly in randomly
    private Vector3 ChangeDirection(Vector3 currentPosition)
    {
        Vector3 newDir;
        if (returnToBase)
        {
            randomizedBase = homeTarget.position;
            randomizedBase.y += Random.Range(-randomBaseOffset, randomBaseOffset);
            newDir = randomizedBase - currentPosition;
        }
        
        else if (distanceFromTarget > radiusMinMax.y)
        {
            newDir = flyingTarget.position - currentPosition;
        }
        
        else if (distanceFromTarget < radiusMinMax.x)
        {
            newDir = currentPosition - flyingTarget.position;
        }

        else 
        {
            // 360-degree freedom of choice on the horizontal plane
            float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
            // Limited max steepness of ascent/descent in the vertical direction
            float angleY = Random.Range(-Mathf.PI / 48f, Mathf.PI / 48f);
            // Calculate direction
            newDir = Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right + Mathf.Sin(angleY) * Vector3.up;
        }

        return newDir.normalized;
    }
}
