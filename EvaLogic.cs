﻿using UnityEngine;
using System.Diagnostics;
using System;

namespace MSD.EvaFollower
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class EvaLogic : MonoBehaviour
    {
        public void Start()
        {
            //EvaDebug.DebugWarning("EvaLogic.Start()");
            
        }
        public void OnDestroy()
        {
            //EvaDebug.DebugWarning("EvaLogic.OnDestroy()");
        }

        public void Update()
        {
            if (!FlightGlobals.ready || PauseMenu.isOpen)
                return;


            try
            {
                foreach (EvaContainer eva in EvaController.fetch.collection.ToArray())
                {
                    if (eva == null)
                    {
                        //is this possible ?
                        EvaDebug.DebugWarning("eva == null");
                        continue;
                    }

                    //skip unloaded vessels
                    if (!eva.Loaded)
                    {
                        continue;
                    }

                    //Turn the lights on when dark.     
                    //Skip for now, too buggy..
                    //eva.UpdateLamps();

                    if (eva.mode == Mode.None)
                    {
                        //Nothing to do here.
                        continue;
                    }

                    //Recover from ragdoll, if possible.    
                    if (eva.IsRagDoll)
                    {
                        eva.RecoverFromRagdoll();
                        continue;
                    }

                    Vector3d move = -eva.Position;

                    //Get next Action, Formation or Patrol
                    Vector3d target = eva.GetNextTarget();

                    // Path Finding
                    //todo: check if the target is occopied.
                    move += target;

                    double sqrDist = move.sqrMagnitude;
                    float speed = TimeWarp.deltaTime;
                    
                    if (eva.OnALadder)
                    {
                        eva.ReleaseLadder();
                    }

                    #region Break Free Code

                    if (eva.IsActive)
                    {
                        Mode mode = eva.mode;

                        if (Input.GetKeyDown(KeyCode.W))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.S))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.A))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.D))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.Q))
                            mode = EvaFollower.Mode.None;
                        if (Input.GetKeyDown(KeyCode.E))
                            mode = EvaFollower.Mode.None;

                        if (mode == Mode.None)
                        {
                            //break free!
                            eva.mode = mode; 
                            continue;
                        }
                    }
                    #endregion

                    //Animation Logic
                    eva.UpdateAnimations(sqrDist, ref speed);

                    move.Normalize();

                    //Distance Logic
                    eva.CheckDistance(move, speed, sqrDist);

                    //Reset Animation Mode Events 
                    eva.CheckModeIsNone();

                }
            }
            catch (Exception exp)
            {
                EvaDebug.DebugWarning("[EFX] EvaLogic: " + exp.Message + ":" + exp.ToString());
            }
        }
    }
}