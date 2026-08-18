import { useMutation } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { authService } from "../../services/authService";
import { FiMail, FiShield } from "react-icons/fi";
import { useLocation } from "react-router-dom";

const OtpVerification = () => {
  const location = useLocation()
  const email = location.state?.email
  const [otp, setOtp] = useState(["", "", "", "", "", ""]);

  const [countdown, setCountdown] = useState(60);

  const inputRefs = useRef([]);

  useEffect(() => {
    if (countdown === 0) {
      return;
    }

    const timer = setInterval(() => {
      setCountdown(prev => prev - 1)
    }, 1000)

    return () => clearInterval(timer);
  });

  const {
    mutate: verifyOtp,
    isPending,
    isError,
  } = useMutation({
    mutationFn: (data) => authService("/auth/verify-email", data),

    onSuccess: () => {
      console.log("OTP verified successfully");
    },

    onError: (error) => {
      console.error(error);
    },
  });

  const handleChange = (value, index) => {
    if (!/^\d?$/.test(value)) {
      return;
    }

    const newOtp = [...otp];
    newOtp[index] = value;

    setOtp(newOtp);

    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus();
    }
  };

  const handleKeyDown = (e, index) => {
    if (e.key === "backspace" && !otp[index] && index > 0) {
      inputRefs.current[index - 1]?.focus();
    }
  };

  const handlePaste = (e) => {
    e.preventDefault();

    const pastedData = e.clipboardData
      .getData("text")
      .replace(/\D/g, "")
      .slice(0, 6);

    if (!pastedData) {
      return;
    }

    const newOtp = [...otp];

    pastedData.split("").forEach((digit, index) => {
      newOtp[index] = digit;
    });

    setOtp(newOtp);

    const nextIndex = Math.min(pastedData.length, 5);

    inputRefs.current[nextIndex]?.focus();
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    const otpValue = otp.join("");

    if (otpValue.length !== 6) {
      return;
    }

    verifyOtp({
      email: email,
      otp: otpValue,
    });
  };

  const handleResend = () => {
    if (countdown > 0) {
      return;
    }

    console.log("Resending OTP");

    setOtp(["", "", "", "", "", ""]);
    setCountdown(60);

    inputRefs.current[0]?.focus();
  };

  return (
    <div className="min-h-screen flex bg-[#f8f6f5] items-center justify-center p-4">
      <div className="w-full max-w-md bg-white rounded-2x1 shadow-xl overflow-hidden">
        {/* header */}
        <div className="px-8 pt-10">
          <div className="flex justify-center mb-5">
            <div className="w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center">
              <FiShield className="w-8 h-8 text-blue-600" />
            </div>
          </div>
          <h2 className="text-3x1 font-bold text-center text-gray-800">
            Verify Your Email
          </h2>

          <p className="text-center text-gray-500 mt-2">
            We've sent a 6-digit verification code to
          </p>

          <div className="flex items-center justify-center gap-2 mt-2">
            <FiMail className="text-blue-600" />
            <span className="font-semibold text-gray-700">
              {email}
            </span>
          </div>
        </div>

        {/* form */}
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* otp input */}
          <div>
            <div className="flex justify-center gap-3" onPaste={handlePaste}>
              {otp.map((digit, index) => (
                <input
                  key={index}
                  ref={(element) => {
                    inputRefs.current[index] = element;
                  }}
                  type="text"
                  inputMode="numeric"
                  maxLength={1}
                  value={digit}
                  onChange={(e) => handleChange(e.target.value, index)}
                  onKeyDown={(e) => handleKeyDown(e, index)}
                  className="w-12 h-14
                      sm:w-14 sm:h-16
                      text-center
                      text-xl
                      font-bold
                      text-gray-700
                      border border-gray-300
                      rounded-xl
                      focus:outline-none
                      focus:ring-2
                      focus:ring-blue-500
                      focus:border-transparent
                      transition-all"
                  autoFocus={index === 0}
                />
              ))}
            </div>

            <p className="text-center text-sm text-gray-400 mt-4">Enter the 6-digit code sent to yout email</p>

          </div>
          
          {/* api error */}
          {isError && (
            <div className="text-center text-sm font-medium text-red-600">Invalid or Expired OTP. Please try again.</div>
          )}

          {/* verify button */}
          <button type="submit" disabled={isPending || otp.join('').length !== 6} 
          className="w-full bg-blue-600 hover:bg-blue-300 disabled:bg-blue-300 text-white font-semibold py-3 rounded-full transition duration-200 shadow-md hover:shadow-lg">
            {isPending ? 'Verifying...' : 'Verify Email'}
          </button>

          {/* resend  */}
          <div className="text-center">
            {countdown > 0 ? (
                <p className="text-sm text-gray-500">Resend code in {' '} <span className="font-semibold text-blue-600">{countdown}s</span></p>
            ):
            (
                <button type="button" onClick={handleResend} className="text-blue-600 font-semibold hover:text-blue-700 text-sm">
                    Resend OTP 
                </button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
};

export default OtpVerification;
