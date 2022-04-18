using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using Utilla;

namespace GrappleMonke
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        //bools
        public bool inAllowedRoom;
        public bool canGrapple;
        public bool hauntedModMenuEnabled = true;
        public bool canPull = false;
        public bool start = true;
        public static ConfigEntry<bool> dp;

        //floats
        public float triggerpressed;
        public float Spring = 20f;
        public float Damper = 0f;
        public float MassScale = 0f;

        //Vectors
        public Vector3 grapplePoint;
        public Vector3 grappleDirection;

        //SpringJoints
        public SpringJoint joint;

        //LineRenderers
        public LineRenderer lr;

        //colors
        public Color grapplecolor = Color.black;

        void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            
        }

        void Update()
        {
            if (inAllowedRoom && hauntedModMenuEnabled)
            {
                List<InputDevice> list = new List<InputDevice>();
                InputDevices.GetDevices(list);

                for (int i = 0; i < list.Count; i++) //Get input
                {
                    if (list[i].characteristics.HasFlag(InputDeviceCharacteristics.Right))
                    {
                        list[i].TryGetFeatureValue(CommonUsages.trigger, out triggerpressed);
                    }
                }

                DrawRope();

                if (start)
                {
                    var child = new GameObject();

                    GameObject.Find("ForestTutorialExit").layer = 2;

                    lr = child.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.startColor = grapplecolor;
                    lr.endColor = grapplecolor;
                    lr.startWidth = 0.02f;
                    lr.endWidth = 0.02f;
                    start = false;
                }

                if(triggerpressed > 0.1f)
                {
                    if (canGrapple)
                    {
                        StartGrapple();
                        canGrapple = false;
                    }

                    StartPull();
                    
                }
                else
                {                  
                    if (!canGrapple)
                    {
                        StopPull();          
                    }
                }

            }
        }


        public void StartGrapple()
        {
            RaycastHit hit;
            if (Physics.Raycast(GorillaLocomotion.Player.Instance.rightHandTransform.position, GorillaLocomotion.Player.Instance.rightHandTransform.forward, out hit, 100f))
            {
                grapplePoint = hit.point;
                grappleDirection = GorillaLocomotion.Player.Instance.rightHandTransform.forward;
                lr.positionCount = 2;
                canPull = true;
            }
        }

        public void StartPull()
        {
            if (canPull)
            {
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = false;

                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.angularVelocity = Vector3.zero;

                GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity = grappleDirection * 10;
            }
        }

        public void StopPull()
        {
            GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.useGravity = true;
            grappleDirection = GorillaLocomotion.Player.Instance.rightHandTransform.forward;
            lr.positionCount = 0;
            canPull = false;
            canGrapple = true;
        }

        public void DrawRope()
        {
            if (!canPull) return;
            lr.SetPosition(0, GorillaLocomotion.Player.Instance.rightHandTransform.position);
            lr.SetPosition(1, grapplePoint);
        }


        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            // The room is modded. Enable mod stuff.


            inAllowedRoom = true;

        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
            GameObject.Find("ForestTutorialExit").layer = 0;
            StopPull();
            start = true;
        }

        void OnEnable()
        {
            hauntedModMenuEnabled = true;
        }

        void OnDisable()
        {
            hauntedModMenuEnabled = false;
            GameObject.Find("ForestTutorialExit").layer = 0;
            StopPull();
            start = true;
        }

    }
}
