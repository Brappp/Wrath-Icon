#region

using System;
using ECommons.EzIpcManager;

#endregion

namespace WrathIcon.Core;

public class WrathIPC
{
    /// <summary>
    /// How often (in milliseconds) to update the Auto-Rotation state.
    /// </summary>
    private static int updateInterval = 500;

    /// <summary>
    /// When <see cref="AutoRotationState"/> was last updated.
    /// </summary>
    private static DateTime lastStateUpdate = DateTime.MinValue;

    /// <summary>
    /// The backing value for <see cref="AutoRotationState"/>.
    /// </summary>
    private static bool? state;

    /// <summary>
    ///     Whether Auto-Rotation is enabled or disabled in Wrath.
    /// </summary>
    /// <remarks>
    ///     Only actual variable that should be used from this class.<br/>
    ///     Only refreshed every occasionally, per <see cref="updateInterval"/>.
    /// </remarks>
    public static bool AutoRotationState
    {
        get
        {
            if (state is null ||
                (DateTime.Now - lastStateUpdate).TotalMilliseconds >= updateInterval)
            {
                state = GetAutoRotationState();
                lastStateUpdate = DateTime.Now;
            }

            return state.Value;
        }
    }

    /// <summary>
    ///     Initiate the IPC connection
    /// </summary>
    private static EzIPCDisposalToken[] _ =
        EzIPC.Init(typeof(WrathIPC), "WrathCombo", SafeWrapper.IPCException);

#pragma warning disable CS8618, CS0649
    /// <summary>
    ///     Method to get Wrath's Auto-Rotation state, whether controlled by IPC or not.
    /// </summary>
    [EzIPC]
    private static readonly Func<bool> GetAutoRotationState;
#pragma warning restore CS8618, CS0649
}
