package com.potatosoft.remote.phoneremote.Utilities;

import java.util.Vector;

/**
 * Created by seanm_000 on 9/18/2017.
 */

public class Publisher<T> {
    private Vector<Subscriber<T>> Subscribers;

    public Publisher() {
        Subscribers = new Vector<>();
    }

    public void Subscribe (Subscriber<T> subscriber) {
        Subscribers.add(subscriber);
    }

    public void Unsubscribe (Subscriber<T> subscriber) {
        Subscribers.remove(subscriber);
    }

    public void Notify(T message) {
        for (Subscriber<T> sub : Subscribers) {
            sub.HandleMessage(message);
        }
    }
}
