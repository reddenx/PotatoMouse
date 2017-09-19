package com.potatosoft.remote.phoneremote.mousepad;

import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by Sean on 4/11/2016.
 */
public class TouchOneUp extends MousePadStateBase {

    private Timer Expiration;
    private final int LEFT_CLICK_UP_WAIT_DURATION = 200;

    public TouchOneUp(MousePadStateBase previousState) {
        super(previousState);
        //timer move to ready and execute click
        Expiration = new Timer();
        Expiration.schedule(new TimerTask() {
            @Override
            public void run() {
                ExecuteClickAndMoveToReady();
            }
        }, LEFT_CLICK_UP_WAIT_DURATION);
    }

    private void ExecuteClickAndMoveToReady() {
        StateHandler.OutputClick();
        ChangeState(new ReadyState(this));
    }

    @Override
    public void TouchUp(float x, float y) {
        //ignore
    }

    @Override
    public void TouchDown(float x, float y) {
        //move to l2d
        Expiration.cancel();
        ChangeState(new TouchOneDownTwo(this, x, y));
    }

    @Override
    public void TouchMove(float x, float y) {
        //ignore
    }
}
