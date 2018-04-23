﻿/*************************************************************************
 *  Copyright © 2015-2018 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  TelescopicJoint.cs
 *  Description  :  Define TelescopicJoint component.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  4/17/2018
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEngine;

namespace Mogoson.Machinery
{
    /// <summary>
    /// Telescopic joint move on the axis Z.
    /// </summary>
    [AddComponentMenu("Mogoson/Machinery/TelescopicJoint")]
    public class TelescopicJoint : TelescopicJointMechanism
    {
        #region Field and Property
        /// <summary>
        /// Local move axis.
        /// </summary>
        protected Vector3 Aixs
        {
            get
            {
                if (transform.parent)
                    return transform.parent.InverseTransformDirection(transform.forward);
                else
                    return transform.forward;
            }
        }
        #endregion

        #region Protected Method
        /// <summary>
        /// Move joint.
        /// </summary>
        /// <param name="moveSpeed">Speed of move joint.</param>
        protected override void DriveJoint(float moveSpeed)
        {
            triggerRecord = Displacement;
            Displacement += moveSpeed * Time.deltaTime;
            Displacement = Mathf.Clamp(Displacement, stroke.min, stroke.max);
            DriveJoint();

            if (CheckLimiterTrigger())
            {
                Displacement = triggerRecord;
                DriveJoint();
            }
        }

        /// <summary>
        /// Move joint.
        /// </summary>
        protected virtual void DriveJoint()
        {
            transform.localPosition = StartPosition + Aixs * Displacement;
            DriveRockers();
        }
        #endregion
    }
}