﻿/*************************************************************************
 *  Copyright © 2015-2018 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  Mechanism.cs
 *  Description  :  Define abstract mechanism.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  2/26/2018
 *  Description  :  Initial development version.
 *************************************************************************/

using System.Collections.Generic;
using Mogoson.MathExtension.Planimetry;
using UnityEngine;

namespace Mogoson.Machinery
{
#if UNITY_EDITOR
    /// <summary>
    /// Custom Edit Mode (Only work in editor script).
    /// </summary>
    public enum EditMode
    {
        Free = 0,
        Hinge = 1,
        Lock = 2
    }
#endif

    /// <summary>
    /// Keep up mode.
    /// </summary>
    public enum KeepUpMode
    {
        TransformUp = 0,
        ReferenceForward = 1
    }

    /// <summary>
    /// Telescopic Mechanism's State.
    /// </summary>
    public enum TelescopicState
    {
        Shrink = 0,
        Drift = 1,
        Extend = 2
    }

    /// <summary>
    /// Mechanism.
    /// </summary>
    public abstract class Mechanism : MonoBehaviour
    {
        /// <summary>
        /// Drive the mechanism.
        /// </summary>
        /// <param name="speedControl">Speed control.</param>
        public abstract void DriveMechanism(float speedControl);
    }

    /// <summary>
    /// Crank Mechanism.
    /// </summary>
	public abstract class CrankMechanism : RockerLockLinkMechanism
    {
        #region Field and Property
        /// <summary>
        /// Crank drive speed.
        /// </summary>
        public float speed = 50;

        /// <summary>
        /// Current angle of crank.
        /// </summary>
        public float Angle { protected set; get; }

        /// <summary>
        /// Start eulerAngles.
        /// </summary>
        public Vector3 StartAngles { protected set; get; }
        #endregion

        #region Protected Method
        /// <summary>
        /// Rotate the crank by angle.
        /// </summary>
        protected virtual void DriveCrank()
        {
            transform.localRotation = Quaternion.Euler(StartAngles + new Vector3(0, 0, Angle));
            DriveRockers();
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Find rocker locks and save start angles.
        /// </summary>
        public new virtual void Awake()
        {
            base.Awake();
            StartAngles = transform.localEulerAngles;
        }
        #endregion
    }

    /// <summary>
    /// Crank Link Mechanism.
    /// </summary>
	public abstract class CrankLinkMechanism : Mechanism
    {
        #region Field and Property
        /// <summary>
        /// Power crank.
        /// </summary>
        public CrankMechanism crank;

        /// <summary>
        /// Gearing link bar.
        /// </summary>
        public RockerMechanism linkBar;

#if UNITY_EDITOR
        /// <summary>
        /// Custom edit mode (Only work in editor script).
        /// </summary>
        [HideInInspector]
        public EditMode editMode = EditMode.Lock;
#endif

        /// <summary>
        /// Dead lock state.
        /// </summary>
        public bool IsLock { protected set; get; }

        /// <summary>
        /// Drive direction.
        /// </summary>
        public bool IsPositive { protected set; get; }

        /// <summary>
        /// Mechanism is initialized.
        /// </summary>
        public bool IsInitialized { protected set; get; }
        #endregion

        #region Protected Method
        /// <summary>
        /// Get link bar position base on this transform.
        /// </summary>
        /// <returns>Local position.</returns>
        protected virtual Vector3 GetLinkPosition()
        {
            return transform.InverseTransformPoint(linkBar.transform.position);
        }

        /// <summary>
        /// Correct position to project point.
        /// </summary>
        /// <param name="position">Local position.</param>
        /// <returns>Correct point.</returns>
        protected Point CorrectPoint(Vector3 position)
        {
            return new Point(position.x, position.y);
        }

        /// <summary>
        /// Clear angles x and y.
        /// </summary>
        /// <param name="angles">Local euler angles.</param>
        /// <returns>Correct angles.</returns>
        protected Vector3 CorrectAngles(Vector3 angles)
        {
            return new Vector3(0, 0, angles.z);
        }

        /// <summary>
        /// Clear position z.
        /// </summary>
        /// <param name="position">Local position.</param>
        /// <returns>Correct position.</returns>
        protected Vector3 CorrectPosition(Vector3 position)
        {
            return new Vector3(position.x, position.y);
        }

        /// <summary>
        /// Drive bars.
        /// </summary>
        protected abstract void DriveLinkBars();
        #endregion

        #region Public Method
        /// <summary>
        /// Initialize mechanism.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Drive the mechanism.
        /// </summary>
        /// <param name="speedControl">Speed control.</param>
        public override void DriveMechanism(float speedControl)
        {
            if (crank.speed * speedControl >= 0)
            {
                if (IsLock && IsPositive)
                    return;
                IsPositive = true;
            }
            else
            {
                if (IsLock && !IsPositive)
                    return;
                IsPositive = false;
            }
            crank.DriveMechanism(speedControl);
            DriveLinkBars();
        }
        #endregion
    }

    /// <summary>
    /// Telescopic Joint Mechanism.
    /// </summary>
	public abstract class TelescopicJointMechanism : RockerLockLinkMechanism
    {
        #region Field and Property
        /// <summary>
        /// Stroke of joint.
        /// </summary>
        public float stroke = 1;

        /// <summary>
        /// Drive speed of joint.
        /// </summary>
        public float speed = 1;

        /// <summary>
        /// Displacement of joint.
        /// </summary>
        public float Displacement { protected set; get; }

        /// <summary>
        /// Telescopic joint state.
        /// </summary>
        public TelescopicState TState { protected set; get; }

        /// <summary>
        /// Start position of joint.
        /// </summary>
        public Vector3 StartPosition { protected set; get; }

        /// <summary>
        /// Local move axis.
        /// </summary>
        protected Vector3 Aixs
        {
            get
            {
                var forward = transform.forward;
                if (transform.parent)
                    forward = transform.parent.InverseTransformDirection(forward);
                return forward;
            }
        }
        #endregion

        #region Protected Method
        protected override void Awake()
        {
            base.Awake();
            StartPosition = transform.localPosition;
            TState = TelescopicState.Shrink;
        }

        /// <summary>
        /// Drive joint.
        /// </summary>
        protected virtual void DriveJoint()
        {
            TState = TelescopicState.Drift;
            transform.localPosition = StartPosition + Aixs * Displacement;
            DriveRockers();
        }
        #endregion
    }

    /// <summary>
    /// Telescopic Arm Mechanism.
    /// </summary>
	public abstract class TelescopicArmMechanism : Mechanism
    {
        #region Field and Property
        /// <summary>
        /// Telescopic joint of arm.
        /// </summary>
        public List<TelescopicJointMechanism> tJoints = new List<TelescopicJointMechanism>();
        #endregion
    }

    /// <summary>
    /// Rocker Mechanism.
    /// </summary>
	public abstract class RockerMechanism : MonoBehaviour
    {
        #region Field and Property
        /// <summary>
        /// Rocker look at joint.
        /// </summary>
        public Transform rockJoint;
        #endregion

        #region Public method
        /// <summary>
        /// Drive the mechanism.
        /// </summary>
        public abstract void DriveMechanism();
        #endregion
    }

    /// <summary>
    /// Rocker Lock Mechanism.
    /// </summary>
    public abstract class RockerLockMechanism : MonoBehaviour
    {
        #region Field and Property
        /// <summary>
        /// Min stroke of the rocker.
        /// </summary>
        public float minStroke = 1;

        /// <summary>
        /// Max stroke of the rocker.
        /// </summary>
        public float maxStroke = 10;

        /// <summary>
        /// Rocker's lock state.
        /// </summary>
        public bool IsLock
        {
            get
            {
                var distance = GetDistance();
                return distance <= minStroke || distance >= maxStroke;
            }
        }

        /// <summary>
        /// Lock target roker joint.
        /// </summary>
        public RockerJoint RJoint { protected set; get; }
        #endregion

        #region Protected Method
        /// <summary>
        /// Find RockerJoint.
        /// </summary>
        protected virtual void Awake()
        {
            RJoint = GetComponent<RockerJoint>();
        }
        #endregion

        #region Public Method
        /// <summary>
        /// Get distance from this transform to rJoint.rockJoint transform.
        /// </summary>
        /// <returns>Distance.</returns>
        public virtual float GetDistance()
        {
            return Vector3.Distance(transform.position, RJoint.rockJoint.position);
        }
        #endregion
    }

    /// <summary>
    /// Rocker Lock Link Mechanism.
    /// </summary>
    public abstract class RockerLockLinkMechanism : Mechanism
    {
        #region Field and Property
        /// <summary>
        /// Rockers that drive by mechanism. 
        /// </summary>
        public List<RockerMechanism> rockers = new List<RockerMechanism>();

        /// <summary>
        /// Rocker locks in this mechanism.
        /// </summary>
        protected List<RockerLockMechanism> rLocks = new List<RockerLockMechanism>();

        /// <summary>
        /// Record lock angle.
        /// </summary>
        protected float lockRecord = 0;
        #endregion

        #region Protected Method
        /// <summary>
        /// Find rocker locks.
        /// </summary>
        protected virtual void Awake()
        {
            foreach (var rocker in rockers)
            {
                var rlock = rocker.GetComponent<RockerLockMechanism>();
                if (rlock)
                    rLocks.Add(rlock);
            }
        }

        /// <summary>
        /// Check rockers's lock state.
        /// </summary>
        /// <returns>Return true if one of the rockers is lock.</returns>
        protected virtual bool CheckRockersLock()
        {
            foreach (var rLock in rLocks)
            {
                if (rLock.IsLock)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Drive the rockers that join at this mechanism.
        /// </summary>
        protected virtual void DriveRockers()
        {
            foreach (var rocker in rockers)
            {
                rocker.DriveMechanism();
            }
        }
        #endregion
    }
}