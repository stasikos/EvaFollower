using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The mode the EVA is in.
    /// </summary>
    [Flags]
    internal enum Mode
    {
        None = 0,
        Follow = 1,
        Patrol = 2,
        Leader = 3,
        Order = 4,
    }

    /// <summary>
    /// The animation states for the EvaControllerContainer
    /// </summary>
    internal enum AnimationState
    {
        None,
        Swim,
        Run,
        Walk,
        BoundSpeed,
        Idle,
    }


}
