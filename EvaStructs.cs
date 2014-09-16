using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSD.EvaFollower
{
    /// <summary>
    /// The mode the EVA is in.
    /// </summary>
    public enum Mode
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
    public enum AnimationState
    {
        None,
        Swim,
        Run,
        Walk,
        BoundSpeed,
        Idle,
    }

    /// <summary>
    /// The formation types avaible...
    /// Don't know how to implement this, yet.
    /// </summary>
    public enum FormationType
    {
        None = 0,
        Column = 1,
        Line = 2, 
        Wedge = 3, 
        Vee = 4,
        Block = 5,
    }

}
