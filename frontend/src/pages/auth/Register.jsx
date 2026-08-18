import React from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import {
  FiLock,
  FiMail,
  FiUser,
  FiPhone
} from 'react-icons/fi'
import { authService } from '../../services/authService'
import { useNavigate } from 'react-router-dom'

const Register = () => {

  const navigate = useNavigate()

  const {
    register,
    handleSubmit,
    watch,
    formState: { errors }
  } = useForm()

  const { mutate, isPending, isError, error } = useMutation({
    mutationFn: (data) => authService('/auth/register', data),
    onSuccess: (_, variables) => {
      navigate('/verify-otp', {
        state: {
          email: variables.email
        }
      })
    }
  })

  const password = watch('password')

  const onSubmit = (data) => {

    const payload = {
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      contactNumber: data.contactNumber,
      username: data.username,
      password: data.password
    }

    mutate(payload)
  }

  return (
    <div className="min-h-screen flex bg-[#f8f6f5] items-center justify-center p-4">

      <div className="w-full max-w-2xl bg-white rounded-2xl shadow-xl overflow-hidden">

        {/* Header */}
        <div className="px-8 pt-8 sm:px-12 sm:pt-10">

          <h2 className="text-3xl font-bold text-center text-gray-800">
            Create Account
          </h2>

          <p className="text-center text-gray-500 mt-2">
            Create your account to get started
          </p>

        </div>

        {/* Form */}
        <div className="p-8 sm:p-12">

          <form
            className="space-y-6"
            onSubmit={handleSubmit(onSubmit)}
          >

            {/* First Name + Last Name */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">

              {/* First Name */}
              <div className="relative pt-2">

                <label
                  htmlFor="firstName"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  First Name
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiUser className="w-5 h-5" />
                  </span>

                  <input
                    {...register('firstName', {
                      required: 'First name is required'
                    })}
                    type="text"
                    id="firstName"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Enter first name"
                    autoComplete="given-name"
                  />

                </div>

                {errors.firstName && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.firstName.message}
                  </span>
                )}

              </div>


              {/* Last Name */}
              <div className="relative pt-2">

                <label
                  htmlFor="lastName"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  Last Name
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiUser className="w-5 h-5" />
                  </span>

                  <input
                    {...register('lastName', {
                      required: 'Last name is required'
                    })}
                    type="text"
                    id="lastName"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Enter last name"
                    autoComplete="family-name"
                  />

                </div>

                {errors.lastName && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.lastName.message}
                  </span>
                )}

              </div>

            </div>


            {/* Email + Contact Number */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">

              {/* Email */}
              <div className="relative pt-2">

                <label
                  htmlFor="email"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  Email Address
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiMail className="w-5 h-5" />
                  </span>

                  <input
                    {...register('email', {
                      required: 'Email is required',
                      pattern: {
                        value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                        message: 'Enter a valid email address'
                      }
                    })}
                    type="email"
                    id="email"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Enter your email"
                    autoComplete="email"
                  />

                </div>

                {errors.email && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.email.message}
                  </span>
                )}

              </div>


              {/* Contact Number */}
              <div className="relative pt-2">

                <label
                  htmlFor="contactNumber"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  Contact Number
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiPhone className="w-5 h-5" />
                  </span>

                  <input
                    {...register('contactNumber', {
                      required: 'Contact number is required'
                    })}
                    type="tel"
                    id="contactNumber"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Enter contact number"
                    autoComplete="tel"
                  />

                </div>

                {errors.contactNumber && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.contactNumber.message}
                  </span>
                )}

              </div>

            </div>


            {/* Username */}
            <div className="relative pt-2">

              <label
                htmlFor="username"
                className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
              >
                Username
              </label>

              <div className="relative">

                <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                  <FiUser className="w-5 h-5" />
                </span>

                <input
                  {...register('username', {
                    required: 'Username is required',
                    minLength: {
                      value: 4,
                      message: 'Username must be at least 4 characters'
                    }
                  })}
                  type="text"
                  id="username"
                  className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                  placeholder="Choose a username"
                  autoComplete="username"
                />

              </div>

              {errors.username && (
                <span className="text-sm font-medium text-red-600 ml-4">
                  {errors.username.message}
                </span>
              )}

            </div>


            {/* Password + Confirm Password */}
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">

              {/* Password */}
              <div className="relative pt-2">

                <label
                  htmlFor="password"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  Password
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiLock className="w-5 h-5" />
                  </span>

                  <input
                    {...register('password', {
                      required: 'Password is required',
                      minLength: {
                        value: 8,
                        message: 'Password must be at least 8 characters'
                      }
                    })}
                    type="password"
                    id="password"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Create password"
                    autoComplete="new-password"
                  />

                </div>

                {errors.password && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.password.message}
                  </span>
                )}

              </div>


              {/* Confirm Password */}
              <div className="relative pt-2">

                <label
                  htmlFor="confirmPassword"
                  className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
                >
                  Confirm Password
                </label>

                <div className="relative">

                  <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                    <FiLock className="w-5 h-5" />
                  </span>

                  <input
                    {...register('confirmPassword', {
                      required: 'Please confirm your password',
                      validate: value =>
                        value === password || 'Passwords do not match'
                    })}
                    type="password"
                    id="confirmPassword"
                    className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    placeholder="Confirm password"
                    autoComplete="new-password"
                  />

                </div>

                {errors.confirmPassword && (
                  <span className="text-sm font-medium text-red-600 ml-4">
                    {errors.confirmPassword.message}
                  </span>
                )}

              </div>

            </div>


            {/* API Error */}
            {isError && (
              <div className="text-center text-sm font-medium text-red-600">
                {error?.message || 'Registration failed. Please try again.'}
              </div>
            )}


            {/* Submit */}
            <button
              type="submit"
              disabled={isPending}
              className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold py-3 rounded-full transition duration-200 shadow-md hover:shadow-lg"
            >
              {isPending ? 'Creating Account...' : 'Create Account'}
            </button>

          </form>


          {/* Login Link */}
          <p className="text-center text-sm text-gray-500 mt-6">
            Already have an account?{' '}
            <button
              type="button"
              className="text-blue-600 font-semibold hover:text-blue-700"
            >
              Sign In
            </button>
          </p>

        </div>

      </div>

    </div>
  )
}

export default Register