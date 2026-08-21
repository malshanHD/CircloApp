import React from 'react'
import { createPortal } from 'react-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from 'react-hook-form'
import { FiX, FiCalendar, FiFileText } from 'react-icons/fi'
import loadingGif from '../../assets/loading.gif'
import { eventService } from '../../services/eventService'

const CreateEventModal = ({ isOpen, onClose }) => {
  const queryClient = useQueryClient()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm()

  const { mutate, isPending, isError, error } = useMutation({
    mutationFn: (data) => eventService.createEvent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] })
      handleClose()
    }
  })

  const onSubmit = (data) => {
    mutate({
      name: data.name,
      description: data.description
    })
  }

  const handleClose = () => {
    reset()
    onClose()
  }

  if (!isOpen) return null

  const errorMessage =
    error?.response?.data?.message ||
    error?.message ||
    'Failed to create event. Please try again.'

  return createPortal(
    /* Outer Backdrop Overlay */
    <div className="fixed inset-0 top-0 left-0 w-screen h-screen z-[99999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      
      {/* Modal Card - Added 'isolate', 'relative z-[100000]', and explicit inline background */}
      <div 
        className="relative z-[100000] isolate w-full max-w-md bg-white rounded-2xl shadow-2xl overflow-hidden max-h-[90vh] overflow-y-auto animate-in fade-in zoom-in duration-200"
        style={{ backgroundColor: '#ffffff' }}
      >
        
        {/* Header */}
        <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-gray-100 bg-white">
          <h3 className="text-xl font-bold text-gray-800">Create New Event</h3>
          <button
            onClick={handleClose}
            type="button"
            className="text-gray-400 hover:text-gray-600 p-1 rounded-full hover:bg-gray-100 transition-colors"
          >
            <FiX className="w-6 h-6" />
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-5 bg-white">
          
          {/* Event Name */}
          <div className="relative pt-2">
            <label
              htmlFor="name"
              className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
            >
              Event Name
            </label>

            <div className="relative">
              <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">
                <FiCalendar className="w-5 h-5" />
              </span>

              <input
                {...register('name', {
                  required: 'Event name is required',
                  minLength: {
                    value: 3,
                    message: 'Name must be at least 3 characters'
                  }
                })}
                type="text"
                id="name"
                className="w-full border border-gray-300 rounded-full pl-12 pr-4 py-3 text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                placeholder="e.g. Summer Vacation Trip"
              />
            </div>

            {errors.name && (
              <span className="text-sm font-medium text-red-600 ml-4">
                {errors.name.message}
              </span>
            )}
          </div>

          {/* Event Description */}
          <div className="relative pt-2">
            <label
              htmlFor="description"
              className="absolute -top-1 left-6 px-1 text-xs font-semibold text-gray-600 bg-white z-10"
            >
              Description
            </label>

            <div className="relative">
              <span className="absolute left-4 top-4 text-gray-400">
                <FiFileText className="w-5 h-5" />
              </span>

              <textarea
                {...register('description', {
                  required: 'Description is required'
                })}
                id="description"
                rows="3"
                className="w-full border border-gray-300 rounded-2xl pl-12 pr-4 py-3 text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all resize-none"
                placeholder="Briefly describe your event..."
              />
            </div>

            {errors.description && (
              <span className="text-sm font-medium text-red-600 ml-4">
                {errors.description.message}
              </span>
            )}
          </div>

          {/* API Error */}
          {isError && (
            <p className="text-center text-sm font-medium text-red-600">
              {errorMessage}
            </p>
          )}

          {/* Action Buttons */}
          <div className="flex gap-3 pt-2">
            <button
              type="button"
              onClick={handleClose}
              disabled={isPending}
              className="w-1/2 py-3 border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold rounded-full transition duration-200"
            >
              Cancel
            </button>

            <button
              type="submit"
              disabled={isPending}
              className="w-1/2 bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-full transition duration-200 shadow-md hover:shadow-lg flex items-center justify-center disabled:opacity-70 h-12"
            >
              {isPending ? (
                <img
                  src={loadingGif}
                  alt="Creating..."
                  className="w-6 h-6 object-contain"
                />
              ) : (
                'Create Event'
              )}
            </button>
          </div>

        </form>

      </div>
    </div>,
    document.body
  )
}

export default CreateEventModal