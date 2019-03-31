using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using ACSL.Interaction;

namespace ACSL.Utility
{
    [Serializable] public class GrabbableEvent : UnityEvent<GrabBase> { }
}