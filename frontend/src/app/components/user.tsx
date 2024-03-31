import { Button } from '@/components/ui/button';
import React, { useEffect, useState } from 'react';

import { supabase } from '../lib/supabase';

import { fakerDE as faker } from '@faker-js/faker';

interface UserProps {
    auth: any;
}

export const User = ({ auth }: UserProps) => {
    const [guestName, setGuestName] = useState('');

    useEffect(() => {
        setGuestName(faker.person.firstName());
    }, []);

    const handleLogout = async () => {
        if (!supabase) throw new Error('Supabase client is not defined');
        const { error } = await supabase.auth.signOut({
            scope: 'local',
        });
    };

    return (
        <>
            <div className='mx-[5%] items-center'>
                <div className='flex gap-3 items-center justify-center'>
                    { Object.keys(auth).length !== 0 ?
                        <>
                            <img src={auth.user_metadata.avatar_url} alt="avatar" width={50} height={50} className='rounded-full' />
                            
                            <p className='text-white text-lg font-medium'>{auth.user_metadata.name}</p>
                            
                            <Button className='bg-transparent' onClick={handleLogout}>
                                <svg width="25" height="25" viewBox="0 0 48 48">
                                    <path fillOpacity='.01' d="M0 0h48v48H0z"/>
                                    <path d="M23.992 6H6v36h18m9-9 9-9-9-9m-17 8.992h26" stroke="#fff" strokeWidth='4' strokeLinecap='round' strokeLinejoin='round'/>
                                </svg>
                            </Button>
                        </>
                    :
                        <>
                            <div className='rounded-full'>
                                <svg height="50px" width="50px">
                                    <circle cx="25" cy="17.81" r="6.58" />
                                    <path d="M25,26.46c-7.35,0-13.3,5.96-13.3,13.3h26.61c0-7.35-5.96-13.3-13.3-13.3Z" />
                                </svg>
                            </div>

                            <p className='text-white text-lg font-medium'>Guest {guestName}</p>
                        </>
                    }
                </div>
            </div>
        </>
    );
}