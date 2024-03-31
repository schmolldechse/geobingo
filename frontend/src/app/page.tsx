'use client';

import socket from './server/socket';
import { SignIn } from "./components/signin";

import { supabase } from './lib/supabase';
import { useEffect } from 'react';

export default function Home() {
    if (!supabase) throw new Error('Supabase client is not defined');

    useEffect(() => {
        async function getUserData() {
            if (!supabase) throw new Error('Supabase client is not defined'); // because of async function?

            await supabase.auth.getUser()
                .then((value) => {
                    console.log(value);
                })
                .catch((error) => {
                    console.log(error);
                });
            };
        getUserData();
    }, []);

    socket.connect();

    socket.on('connect', () => {
        console.log('connected with server');
        socket.emit('connection', 'hello from client');
    });

    socket.on('error', (error) => {
        console.log('error while connecting to server', error);
    });

    return (
        <>
            <div className='flex items-center justify-center h-screen w-screen'>
                <SignIn />
            </div>
        </>
    );
}
