'use client';

import socket from './server/socket';

import { supabase } from './lib/supabase';
import { useEffect, useState } from 'react';
import Landing from './sites/landing';

export default function Home() {
    if (!supabase) throw new Error('Supabase client is not defined');

    const [auth, setAuth] = useState({});

    useEffect(() => {
        async function getUserData() {
            if (!supabase) throw new Error('Supabase client is not defined'); // because of async function?

            const { data, error} = await supabase.auth.getSession();
            if (error) throw new Error('Error while retrieving user data:', error);

            console.log('Retrieved user data:', data);
        };

        getUserData();
    }, []);

    supabase.auth.onAuthStateChange(async (event, session) => {
        if (event === 'SIGNED_IN' && session) {
            setAuth(session.user);
        } else if (event === 'SIGNED_OUT') {
            setAuth({});
        }
    });

    socket.on('connect', () => {
        console.log('connected with server');
        socket.emit('connection', 'hello from client');
    });

    socket.on('error', (error) => {
        console.log('error while connecting to server', error);
    });

    return (
        <>
            <Landing auth={auth} />
        </>
    );
}
