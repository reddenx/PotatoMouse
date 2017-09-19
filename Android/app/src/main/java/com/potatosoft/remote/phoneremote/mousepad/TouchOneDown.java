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

    public TouchOneDown(MousePadStateBase previousState, float x, float y){
        super(previousState);
        Expiration = new Timer();
        Expiration.schedule(new TimerTask() {
            @Override
            public void run() {
                SwitchToMove();
            }
        }, LEFT_CLICK_TO_MOVE_WAIT);
        StartX = x;
        StartY = y;
    }

    private void SwitchToMove(){
        ChangeState(new MoveState(this, StartX, StartY));
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
        if(Math.abs(StartX - x) > LEFT_CLICK_MOVE_THRESHOLD || Math.abs(StartY - y) > LEFT_CLICK_MOVE_THRESHOLD) {
            Expiration.cancel();
            SwitchToMove();
        }
    }
}
