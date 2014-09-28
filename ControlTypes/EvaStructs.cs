using System;
using System.Collections.Generic;
using System.Text;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The mode the EVA is in.
    /// </summary>
    /// 
    [Flags]
    enum Mode
    {
        None = 1,
        Follow = 2,
        Patrol = 3,
        Leader = 4,
        Order = 5,
    }

    /// <summary>
    /// The status the EVA is in.
    /// </summary>
    /// 
    [Flags]
    enum Status
    {
        None,
        Removed
    }

    /// <summary>
    /// The animation states for the EvaControllerContainer
    /// </summary>
    enum AnimationState
    {
        None,
        Swim,
        Run,
        Walk,
        BoundSpeed,
        Idle,
    }


}
