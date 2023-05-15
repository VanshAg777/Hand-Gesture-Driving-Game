using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolvoCars.Data;
using DigitalRuby.Tween;
using System;

namespace VolvoCars.Behaviour
{
    public class DoorController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Degrees per second.")]
        [Range(0, 200)]
        public float doorCloseRate = 90.0f;
        [Tooltip("Degrees per second.")]
        [Range(0, 150)]
        public float doorOpenRate = 30.0f;
        [Tooltip("How much is the door opened when triggered by UI input.")]
        public float doorOpenAngle = 0.0f;

        [Header("References")]
        public Rotator doorRotator;

        [Header("Sound References")]
        public AudioClip openDoorExtSound;
        public AudioClip openDoorIntSound;
        public AudioClip closeDoorExtSound;
        public AudioClip closeDoorIntSound;
        public AudioSource soundSource;

        [Header("Optional Co-rotator Object")]
        [Tooltip("Additional object that should co-rotate, such as a parcel shelf for the tailgate. This is optional.")]
        public GameObject corotatorGameObj;
        [Tooltip("Additional object that should co-rotate, such as a parcel shelf for the tailgate. This is optional.")]
        public Vector3 corotatorOpenRotation = new Vector3(0, 45, 0);
        [Range(0, 0.95f)]
        public float corotatorOpenDelay = 0;

        [Header("Data")]
        public GenericData doorIsOpen;
        public CameraIsInsideCar cameraIsInsideCar;

        private bool userInside;
        private Vector3 followerClosedRotation = new Vector3(0, 0, 0);
        private bool animateFollower = false;
        private bool doorViewedAsClosed = true; // The state according to latest animation
        private bool doorIsOpenVal = false;

        private Action<bool> cameraIsInsideCarAction;
        private Action<object> doorIsOpenAction;


        void Start()
        {
            UnityEngine.Assertions.Assert.IsNotNull(doorRotator, "Missing rotator reference in DoorController: " + gameObject.name);

            if (animateFollower = corotatorGameObj != null)
                followerClosedRotation = corotatorGameObj.transform.localEulerAngles;

            cameraIsInsideCarAction = (v) => userInside = v;
            cameraIsInsideCar.Subscribe(cameraIsInsideCarAction);

            doorIsOpenAction = (obj) => doorIsOpenVal = (bool)obj;
            doorIsOpen.SubscribeImmediate(doorIsOpenAction);
        }


        void FixedUpdate()
        {
            if (doorViewedAsClosed && doorIsOpenVal)
            {
                TweenAngle(doorRotator.angle, doorOpenAngle, doorOpenRate);
                doorViewedAsClosed = false;
                if (userInside)
                {
                    soundSource.PlayOneShot(openDoorIntSound);
                }
                else
                {
                    soundSource.PlayOneShot(openDoorExtSound);
                }
            }
            else if (!doorViewedAsClosed && !doorIsOpenVal)
            {
                TweenAngle(doorRotator.angle, doorRotator.min, doorCloseRate);
                doorViewedAsClosed = true;
            }
        }

        private void OnDestroy()
        {
            cameraIsInsideCar.Unsubscribe(cameraIsInsideCarAction);
            doorIsOpen.Unsubscribe(doorIsOpenAction);
        }

        private void TweenAngle(float fromAngle, float toAngle, float rate)
        {
            if (doorRotator == null)
                return;

            // Smooth start and end if closure is opened, abrupt end if closed
            System.Func<float, float> animationProgression;
            if (toAngle == 0)
            {
                animationProgression = TweenScaleFunctions.QuadraticEaseIn;
            }
            else
            {
                animationProgression = TweenScaleFunctions.QuadraticEaseInOut;
            }

            // Calculate duration
            float duration = 0.0f;
            float diff = Mathf.Abs(toAngle - fromAngle) % 360;
            if (rate != 0)
                duration = diff / rate;

            doorRotator.gameObject.Tween(doorRotator.gameObject, fromAngle, toAngle, duration, animationProgression, (t) =>
            {
                // Progress
                doorRotator.Move(t.CurrentValue);
            }, (t) =>
            {
                // Completion - Play close door sound
                if (toAngle == doorRotator.min)
                {
                    soundSource.PlayOneShot(userInside ? closeDoorIntSound : closeDoorExtSound);
                }
            });

            //Animation of additional object
            if (animateFollower)
                StartCoroutine(TweenFollower(toAngle == doorRotator.max, duration, animationProgression));
        }

        IEnumerator TweenFollower(bool goToOpen, float parentDuration, System.Func<float, float> animationProgression)
        {
            if (animateFollower)
            {
                float delay = corotatorOpenDelay * parentDuration;
                Quaternion endValue = Quaternion.Euler(followerClosedRotation);
                if (goToOpen)
                {
                    endValue = Quaternion.Euler(corotatorOpenRotation);
                    yield return new WaitForSeconds(delay);
                }
                corotatorGameObj.Tween(corotatorGameObj, corotatorGameObj.transform.localRotation, endValue, parentDuration - delay, animationProgression, (t) =>
                {
                    // Progress
                    corotatorGameObj.transform.localRotation = t.CurrentValue;
                }, (t) =>
                {
                    // Completion
                });
            }
            else
            {
                yield return null;
            }
        }

    }
}