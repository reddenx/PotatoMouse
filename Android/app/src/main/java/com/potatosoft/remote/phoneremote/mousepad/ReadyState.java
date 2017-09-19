package com.potatosoft.remote.phoneremote.mousepad;

/**
 * Created by Sean on 4/11/2016.
 */
public class ReadyState extends MousePadStateBase {
    public ReadyState(MousepadStateHandler container) {
        super(container);
    }

    public ReadyState(MousePadStateBase previousState) {
        super(previousState);
    }

    @Override
    public void TouchUp(float x, float y) {
        //ignore
    }

    @Override
    public void TouchDown(float x, float y) {
        this.ChangeState(new TouchOneDown(this, x, y));
    }

    @Override
    public void TouchMove(float x, float y) {
        //ignore
    }
}
