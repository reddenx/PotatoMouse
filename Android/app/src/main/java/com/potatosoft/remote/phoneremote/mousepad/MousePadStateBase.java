package com.potatosoft.remote.phoneremote.mousepad;

/**
 * Created by Sean on 4/11/2016.
 */
public abstract class MousePadStateBase {

    protected MousepadStateHandler StateHandler;

    public MousePadStateBase(MousepadStateHandler handler) {
        StateHandler = handler;
    }

    protected MousePadStateBase(MousePadStateBase previousState) {
        this.StateHandler = previousState.StateHandler;
    }

    protected void ChangeState(MousePadStateBase newState) {
        this.StateHandler.SetState(newState);
    }

    public abstract void TouchUp(float x, float y);
    public abstract void TouchDown(float x, float y);
    public abstract void TouchMove(float x, float y);
}