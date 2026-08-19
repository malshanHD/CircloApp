import { useMutation, useQueryClient } from '@tanstack/react-query'
import React, { useState } from 'react'
import { useForm } from 'react-hook-form'
import { FiLock, FiMail } from 'react-icons/fi'
import { authService } from '../../services/authService'
import { useNavigate } from 'react-router-dom'

const Login = () => {

    const navigate = useNavigate();

    const [payload, setPayload] = useState({});

    const {mutate, isLoading, isError} = useMutation({
        mutationFn: () => authService('/auth/login', payload),
        onSuccess: (response)=> {
            const token = response.data.accessToken;
            console.log(token);
            localStorage.setItem('accessToken', token);
            navigate('/dashboard');
        }
    });

    const {register, handleSubmit, formState: {errors}} = useForm();
    
    const onSubmit = (data) => {
        console.log('form data', data);
        setPayload(data);
        mutate(data);
    }

  return (
    <div className="min-h-screen flex bg-[#f8f6f5] items-center justify-center p-4">
      <div className="flex max-w-xl w-full bg-white rounded-2xl overflow-hidden shadow-xl">
        {/* Login Form */}
        <div className="flex-1 p-8 sm:p-12 flex flex-col justify-center">
          <h2 className="text-3xl font-bold mb-8 text-center text-gray-800">Welcome Back</h2>
          
          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            {/* Email Field */}
            <div className="relative pt-2">
              <label 
                htmlFor="email" 
                className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
              >
                Email Address
              </label>
              
              <div className="relative">
                <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400">
                  <FiMail className="w-5 h-5" />
                </span>
                <input {...register("usernameOrEmail", {required: "Email is required"})}
                  type="email" 
                  id="email"
                  className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="Enter your email"
                  autoComplete="off"
                />
                {errors.email && <span className='font-bold text-red-700'>{errors.email.message}</span>}
              </div>
            </div>

            {/* Password Field */}
            <div className="relative pt-2">
              <label 
                htmlFor="password" 
                className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
              >
                Password
              </label>
              
              <div className="relative">
                <span className="absolute left-4 top-1/2 transform -translate-y-1/2 text-gray-400">
                  <FiLock className="w-5 h-5" />
                </span>
                <input {...register('password', {required: "Password is required"})}
                  type="password" 
                  id="password"
                  className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="Enter your password"
                  autoComplete="off"
                />
                {errors.password && <span>{errors.email.password}</span>}
              </div>
            </div>

            {/* Submit Button */}
            <button 
              type="submit" 
              className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-full transition duration-200 shadow-md hover:shadow-lg"
            >
              Sign In
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}

export default Login