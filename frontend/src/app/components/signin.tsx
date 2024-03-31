import { Button } from '@/components/ui/button';
import React from 'react';

import { supabase } from '../lib/supabase';

export const SignIn = () => {

    const handleSignIn = async () => {
        if (!supabase) throw new Error('Supabase client is not defined');

        const { data, error } = await supabase.auth.signInWithOAuth({
            provider: 'twitch',
        });
    };

    return (
        <>
        <Button 
            className='bg-[#653DA2] rounded-[10px] p-3 py-6 flex gap-3 items-center inline-flex hover:ring-4 hover:ring-[#522891] hover:bg-[#7E58C2]'
            onClick={handleSignIn}
        >
            <svg width="30px" height="30px" viewBox="0 0 16 16" fill="none">
                <g fill="#000">
                    <path d="M4.5 1 2 3.5v9h3V15l2.5-2.5h2L14 8V1zM13 7.5l-2 2H9l-1.75 1.75V9.5H5V2h8z"/>
                    <path d="M11.5 3.75h-1v3h1zm-2.75 0h-1v3h1z"/>
                </g>
            </svg>

            <p className='text-black text-lg'>Continue with Twitch</p>
        </Button>
        </>
    );
}