package com.potatosoft.remote.phoneremote.mousepad;

import java.util.Timer;
import java.util.TimerTask;

/**
 * Created by Sean on 4/11/2016.
 */
public class TouchOneDown extends MousePadStateBase {

    private Timer Expiration;
    private final int LEFT_CLICK_TO_MOVE_WAIT = 300;
    private final int LEFT_CLICK_MOVE_THRESHOLD = 20;
    private float StartX;
    private float StartY;
    private float LastX;
    private float LastY;

    public TouchOneDown(MousePadStateBase previousState, float x, float y){
        super(previousState);
        Expiration = new Timer();

        StartX = x;
        StartY = y;
        LastX  = x;
        LastY = y;

        Expiration.schedule(new TimerTask() {
            @Override
            public void run() {
                SwitchToMove();
            }
        }, LEFT_CLICK_TO_MOVE_WAIT);
    }

    private void SwitchToMove(){
        ChangeState(new MoveState(this, LastX, LastY));
    }

    @Override
    public void TouchUp(float x, float y) {
        Expiration.cancel();
        ChangeState(new TouchOneUp(this));
    }

    @Override
    public void TouchDown(float x, float y) {
        //ignore
    }

    @Override
    public void TouchMove(float x, float y) {
        //check if went outside zone
        LastX = x;
        LastY = y;
        if(Math.abs(StartX - x) > LEFT_CLICK_MOVE_THRESHOLD || Math.abs(StartY - y) > LEFT_CLICK_MOVE_THRESHOLD) {
            Expiration.cancel();
            SwitchToMove();
        }
    }
}
