using System;
using UnityEngine;

public abstract class CPlayerActionState : IAnimChecker
{
    public abstract void fEnterState(CPlayerCtrl3 player);
    public abstract CPlayerActionState fUpdateState(CPlayerCtrl3 player);
    public abstract void fExitState(CPlayerCtrl3 player);

    public abstract void fCheckOnStateEnter(ref AnimatorStateInfo stateInfo);
    public abstract void fCheckOnStateExit(ref AnimatorStateInfo stateInfo);
}
