package com.potatosoft.remote.phoneremote.Utilities;

/**
 * Created by seanm_000 on 9/18/2017.
 */

public abstract class Subscriber<T> {
    public abstract void HandleMessage(T message);
}