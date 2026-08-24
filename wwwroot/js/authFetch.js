// authFetch.js
// Consolidated JavaScript for authentication and authenticated API calls.

/**
 * Makes an authenticated fetch request by including the JWT token from localStorage.
 * Redirects to the login page if no token is found or if authentication fails (401/403).
 * @param {string} url The URL to fetch.
 * @param {object} options Fetch options (method, headers, body, etc.).
 * @returns {Promise<Response>} A Promise that resolves with the fetch Response.
 * @throws {Error} If a network error occurs or authentication fails.
 */
async function authFetch(url, options = {}) {
    const token = localStorage.getItem('jwtToken');

    if (!token) {
        console.warn("authFetch: No JWT token found in localStorage. Redirecting to login.");
        // Redirect to login page, including the current page as ReturnUrl
        window.location.href = `/Account/Login?ReturnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`;
        // Throw an error to stop the current operation
        throw new Error("Authentication token not found.");
    }

    // Add or merge the Authorization header with the Bearer token
    const headers = {
        ...options.headers, // Keep existing headers
        'Authorization': `Bearer ${token}` // Add or overwrite Authorization header
    };

    // Create new options object with the updated headers
    const authOptions = {
        ...options, // Keep existing options
        headers: headers // Use the updated headers
    };

    try {
        // Perform the fetch request
        const response = await fetch(url, authOptions);

        // Handle specific authentication errors (e.g., 401 Unauthorized, 403 Forbidden)
        if (response.status === 401 || response.status === 403) {
            console.error(`authFetch: Authentication failed with status ${response.status}. Token might be expired or invalid.`);
            // Clear the invalid token and redirect to login
            localStorage.removeItem('jwtToken');
            window.location.href = `/Account/Login?ReturnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`;
            throw new Error("Authentication failed. Please log in again.");
        }

        return response; // Return the response for further processing

    } catch (error) {
        console.error("authFetch: Network error or exception during fetch:", error);
        // Re-throw the error so calling code can handle it
        throw error;
    }
}

/**
 * Parses the error response from an API call, attempting to extract messages
 * from a general 'message' property or ModelState 'errors'.
 * @param {Response} response The fetch Response object.
 * @returns {Promise<string>} A Promise that resolves with a formatted error message string.
 */
async function parseApiResponseError(response) {
    try {
        const errorData = await response.json();
        let errorMessage = `API request failed with status ${response.status}.`; // Default message

        if (errorData) {
            if (errorData.message) { // Check for a general message property
                errorMessage = errorData.message;
            } else if (errorData.errors && typeof errorData.errors === 'object') {
                // Handle validation errors from ModelState
                let validationErrors = [];
                for (const field in errorData.errors) {
                    if (errorData.errors.hasOwnProperty(field)) {
                        // Concatenate field name (if applicable) and error messages
                        const fieldName = field === '' ? '' : `${field.replace('Input.', '').replace('.', ' ')}: `; // Clean up field name for display
                        validationErrors = validationErrors.concat(errorData.errors[field].map(msg => `${fieldName}${msg}`));
                    }
                }
                if (validationErrors.length > 0) {
                    errorMessage = "Validation errors: " + validationErrors.join(' '); // Join multiple validation messages
                } else if (errorData.title) {
                     // Handle other potential error structures (e.g., default API error response)
                      errorMessage = errorData.title;
                 }
            } else if (typeof errorData === 'string') {
                 // Handle plain text error responses
                 errorMessage = errorData;
            }
        }
        return errorMessage;
    } catch (parseError) {
        console.error("Failed to parse API error response:", parseError);
        return `API request failed with status ${response.status}. Could not parse error details.`;
    }
}


/**
 * Handles the login form submission by making an API call, storing the token, and redirecting.
 * @param {HTMLFormElement} formElement The login form DOM element.
 * @param {HTMLElement} errorDisplayElement The element to display error messages in.
 */
async function handleLogin(formElement, errorDisplayElement) {
    // Prevent the default browser form submission
    event.preventDefault();

    // Clear previous error messages
    if (errorDisplayElement) {
        errorDisplayElement.textContent = '';
        errorDisplayElement.style.display = 'none'; // Hide the error div
    }

    // Get input values using the correct IDs from your Login.cshtml
    const emailInput = formElement.querySelector('#Input_Email');
    const passwordInput = formElement.querySelector('#Input_Password');
    const rememberMeInput = formElement.querySelector('#Input_RememberMe'); // Assuming this ID

    if (!emailInput || !passwordInput || !rememberMeInput) {
         console.error("Login form inputs not found within the provided form element!");
         if (errorDisplayElement) {
              errorDisplayElement.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
              errorDisplayElement.style.display = 'block';
         }
         return; // Stop if inputs aren't found
    }

    const email = emailInput.value;
    const password = passwordInput.value;
    const rememberMe = rememberMeInput.checked;

    // Simple client-side validation check (more robust validation should be server-side)
    if (!email || !password) {
        if (errorDisplayElement) {
            errorDisplayElement.textContent = "Please enter both email and password.";
            errorDisplayElement.style.display = 'block';
        }
        return;
    }


    // Create the data payload matching your C# LoginModel DTO
    const loginData = {
        email: email,
        password: password,
        rememberMe: rememberMe
    };

    const url = '/api/Account/Login'; // Your login API endpoint URL
    const options = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json' // Specify content type as JSON
        },
        body: JSON.stringify(loginData) // Convert data object to JSON string
    };

    try {
        // Make the API call using the fetch API
        const response = await fetch(url, options);

        // Check if the response was successful (status code 2xx)
        if (response.ok) {
            console.log("Login successful!");
            const data = await response.json(); // Parse the JSON response body

            // Extract and store the JWT token
            const token = data.token; // Assuming the API returns { token: "..." }
            if (token) {
                localStorage.setItem('jwtToken', token); // Store the token in local storage
                console.log("JWT Token stored:", token);

                // Redirect the user
                const params = new URLSearchParams(window.location.search);
                const returnUrl = params.get('ReturnUrl');

                if (returnUrl) {
                    console.log(`Redirecting to ReturnUrl: ${returnUrl}`);
                    window.location.href = returnUrl; // Redirect to the original page
                } else {
                    console.log("No ReturnUrl, redirecting to default page /Book/Restaurant");
                    window.location.href = '/Book/Restaurant'; // Your desired default page
                }

            } else {
                 console.error("Login successful but no token received.");
                 if (errorDisplayElement) {
                      errorDisplayElement.textContent = "Login successful, but failed to get token. Please try again.";
                      errorDisplayElement.style.display = 'block';
                 }
            }

        } else {
            // Login failed (HTTP status code is not 2xx)
            console.error("Login failed with status:", response.status);
            const errorMessage = await parseApiResponseError(response);
            if (errorDisplayElement) {
                errorDisplayElement.textContent = errorMessage;
                errorDisplayElement.style.display = 'block'; // Show the error div
            } else {
                 alert(errorMessage); // Fallback
            }
        }

    } catch (error) {
        console.error("An error occurred during login fetch:", error);
        if (errorDisplayElement) {
            errorDisplayElement.textContent = 'A network error occurred. Please try again.';
            errorDisplayElement.style.display = 'block'; // Show the error div
        } else {
             alert('A network error occurred. Please try again.');
        }
    }
}

/**
 * Handles the registration form submission.
 * @param {HTMLFormElement} formElement The registration form DOM element.
 * @param {HTMLElement} messageDisplayElement The element to display messages (success or error) in.
 */
async function handleRegister(formElement, messageDisplayElement) {
     event.preventDefault();

     if (messageDisplayElement) {
         messageDisplayElement.textContent = '';
         messageDisplayElement.style.display = 'none';
         messageDisplayElement.style.color = ''; // Reset color
     }

     const emailInput = formElement.querySelector('#Input_Email');
     const passwordInput = formElement.querySelector('#Input_Password');
     const confirmPasswordInput = formElement.querySelector('#Input_ConfirmPassword');

     if (!emailInput || !passwordInput || !confirmPasswordInput) {
          console.error("Registration form inputs not found!");
           if (messageDisplayElement) {
                messageDisplayElement.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
                messageDisplayElement.style.color = 'red';
                messageDisplayElement.style.display = 'block';
           }
          return;
     }

     const email = emailInput.value;
     const password = passwordInput.value;
     const confirmPassword = confirmPasswordInput.value;

     // Simple client-side validation
     if (!email || !password || !confirmPassword) {
         if (messageDisplayElement) {
              messageDisplayElement.textContent = "Please fill in all fields.";
              messageDisplayElement.style.color = 'red';
              messageDisplayElement.style.display = 'block';
         }
         return;
     }
     if (password !== confirmPassword) {
         if (messageDisplayElement) {
              messageDisplayElement.textContent = "Password and confirmation password do not match.";
              messageDisplayElement.style.color = 'red';
              messageDisplayElement.style.display = 'block';
         }
         return;
     }


     const registerData = {
         email: email,
         password: password,
         confirmPassword: confirmPassword
     };

     const url = '/api/Account/Register'; // Your registration API endpoint URL
     const options = {
         method: 'POST',
         headers: {
             'Content-Type': 'application/json'
         },
         body: JSON.stringify(registerData)
     };

     console.log('Attempting registration for:', email);

     try {
         const response = await fetch(url, options);

         if (response.ok) {
             console.log("Registration successful!");
             const data = await response.json();
             let successMessage = data.message || "Registration successful!";

             if (messageDisplayElement) {
                  messageDisplayElement.textContent = successMessage;
                  messageDisplayElement.style.color = 'green';
                  messageDisplayElement.style.display = 'block';
             } else {
                  alert(successMessage);
             }

             // Optional: Redirect to login page after successful registration
             // setTimeout(() => { window.location.href = '/Account/Login'; }, 3000); // Redirect after 3 seconds

         } else {
             console.error("Registration failed with status:", response.status);
             const errorMessage = await parseApiResponseError(response);
             if (messageDisplayElement) {
                  messageDisplayElement.textContent = errorMessage;
                  messageDisplayElement.style.color = 'red';
                  messageDisplayElement.style.display = 'block';
             } else {
                  alert(errorMessage);
             }
         }

     } catch (error) {
         console.error("An error occurred during registration fetch:", error);
         if (messageDisplayElement) {
              messageDisplayElement.textContent = 'A network error occurred. Please try again.';
              messageDisplayElement.style.color = 'red';
              messageDisplayElement.style.display = 'block';
         } else {
              alert('A network error occurred. Please try again.');
         }
     }
}

/**
 * Handles the forgot password form submission.
 * @param {HTMLFormElement} formElement The forgot password form DOM element.
 * @param {HTMLElement} messageDisplayElement The element to display messages (success or error) in.
 */
async function handleForgotPassword(formElement, messageDisplayElement) {
     event.preventDefault();

     if (messageDisplayElement) {
         messageDisplayElement.textContent = '';
         messageDisplayElement.style.display = 'none';
         messageDisplayElement.style.color = ''; // Reset color
     }

     const emailInput = formElement.querySelector('#Input_Email'); // Assuming this ID

      if (!emailInput) {
          console.error("Forgot password email input not found!");
           if (messageDisplayElement) {
                messageDisplayElement.textContent = "Error: Email input not found.";
                messageDisplayElement.style.color = 'red';
                messageDisplayElement.style.display = 'block';
           }
          return;
      }

     const email = emailInput.value;

     if (!email) {
          if (messageDisplayElement) {
               messageDisplayElement.textContent = "Please enter your email address.";
               messageDisplayElement.style.color = 'red';
               messageDisplayElement.style.display = 'block';
          }
          return;
     }

     const forgotPasswordData = {
         email: email
     };

     const url = '/api/Account/ForgotPassword'; // Your forgot password API endpoint URL
     const options = {
         method: 'POST',
         headers: {
             'Content-Type': 'application/json'
         },
         body: JSON.stringify(forgotPasswordData)
     };

     console.log('Attempting to send password reset link to:', email);

     try {
          const response = await fetch(url, options);
          const result = await response.json(); // Assuming API returns JSON

          if (response.ok) {
               console.log("Forgot password request successful:", result);
                if (messageDisplayElement) {
                     messageDisplayElement.textContent = result.message || 'Password reset link sent. Please check your email.';
                     messageDisplayElement.style.color = 'green';
                     messageDisplayElement.style.display = 'block';
                } else {
                     alert(result.message || 'Password reset link sent. Please check your email.');
                }
          } else {
               console.error("Forgot password request failed:", response.status, result);
               const errorMessage = await parseApiResponseError(response);
                if (messageDisplayElement) {
                     messageDisplayElement.textContent = errorMessage;
                     messageDisplayElement.style.color = 'red';
                     messageDisplayElement.style.display = 'block';
                } else {
                     alert(errorMessage);
                }
          }

     } catch (error) {
          console.error("An error occurred during forgot password fetch:", error);
           if (messageDisplayElement) {
                messageDisplayElement.textContent = 'A network error occurred. Please try again.';
                messageDisplayElement.style.color = 'red';
                messageDisplayElement.style.display = 'block';
           } else {
                alert('A network error occurred. Please try again.');
           }
     }
}

/**
 * Handles the reset password form submission.
 * @param {HTMLFormElement} formElement The reset password form DOM element.
 * @param {HTMLElement} messageDisplayElement The element to display messages (success or error) in.
 */
async function handleResetPassword(formElement, messageDisplayElement) {
     event.preventDefault();

      if (messageDisplayElement) {
          messageDisplayElement.textContent = '';
          messageDisplayElement.style.display = 'none';
          messageDisplayElement.style.color = ''; // Reset color
      }

     const emailInput = formElement.querySelector('#Input_Email');
     const tokenInput = formElement.querySelector('#Input_Token'); // The hidden input for the token
     const newPasswordInput = formElement.querySelector('#Input_NewPassword');
     const confirmPasswordInput = formElement.querySelector('#Input_ConfirmPassword');

      if (!emailInput || !tokenInput || !newPasswordInput || !confirmPasswordInput) {
           console.error("Reset password form inputs not found!");
            if (messageDisplayElement) {
                 messageDisplayElement.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
                 messageDisplayElement.style.color = 'red';
                 messageDisplayElement.style.display = 'block';
            }
           return;
      }

     const email = emailInput.value;
     const token = tokenInput.value;
     const newPassword = newPasswordInput.value;
     const confirmPassword = confirmPasswordInput.value;

     // Simple client-side validation
     if (!email || !token || !newPassword || !confirmPassword) {
         if (messageDisplayElement) {
              messageDisplayElement.textContent = "Please fill in all fields.";
              messageDisplayElement.style.color = 'red';
              messageDisplayElement.style.display = 'block';
         }
         return;
     }
     if (newPassword !== confirmPassword) {
         if (messageDisplayElement) {
              messageDisplayElement.textContent = "New password and confirmation password do not match.";
              messageDisplayElement.style.color = 'red';
              messageDisplayElement.style.display = 'block';
         }
         return;
     }


     const resetPasswordData = {
         email: email,
         token: token,
         newPassword: newPassword,
         confirmPassword: confirmPassword
     };

     const url = '/api/Account/ResetPassword'; // Your reset password API endpoint URL
     const options = {
         method: 'POST',
         headers: {
             'Content-Type': 'application/json'
         },
         body: JSON.stringify(resetPasswordData)
     };

     console.log('Attempting to reset password for:', email);

     try {
          const response = await fetch(url, options);
          const result = await response.json(); // Assuming API returns JSON

          if (response.ok) {
               console.log("Password reset successful:", result);
                if (messageDisplayElement) {
                     messageDisplayElement.textContent = result.message || 'Your password has been reset successfully.';
                     messageDisplayElement.style.color = 'green';
                     messageDisplayElement.style.display = 'block';

                     // Optionally clear the form fields on success
                     // newPasswordInput.value = '';
                     // confirmPasswordInput.value = '';

                     // Optionally redirect to the login page after a delay
                     // setTimeout(() => { window.location.href = '/Account/Login'; }, 3000); // Redirect after 3 seconds
                } else {
                     alert(result.message || 'Your password has been reset successfully.');
                }
          } else {
               console.error("Password reset failed:", response.status, result);
               const errorMessage = await parseApiResponseError(response);
                if (messageDisplayElement) {
                     messageDisplayElement.textContent = errorMessage;
                     messageDisplayElement.style.color = 'red';
                     messageDisplayElement.style.display = 'block';
                } else {
                     alert(errorMessage);
                }
          }

     } catch (error) {
          console.error("An error occurred during reset password fetch:", error);
           if (messageDisplayElement) {
                messageDisplayElement.textContent = 'A network error occurred. Please try again.';
                messageDisplayElement.style.color = 'red';
                messageDisplayElement.style.display = 'block';
           } else {
                alert('A network error occurred. Please try again.');
           }
     }
}


/**
 * Handles the click event for editing a Restaurant booking.
 * Redirects to the MyBookings page with the booking ID in the query string.
 * @param {string} bookingId The GUID of the restaurant booking to edit.
 */
function handleEditRestaurantBooking(bookingId) {
    console.log(`Initiating edit for Restaurant Booking ID: ${bookingId}`);
    // Redirect to the current page with the editRestaurantBookingId query parameter
    window.location.href = `/Book/MyBookings?editRestaurantBookingId=${bookingId}`;
}

/**
 * Handles the click event for canceling a Restaurant booking.
 * Makes an authenticated API call to cancel the booking.
 * @param {string} bookingId The GUID of the restaurant booking to cancel.
 */
async function handleCancelRestaurantBooking(bookingId) {
    console.log(`Attempting to cancel Restaurant Booking ID: ${bookingId}`);

    // Optional: Show a confirmation dialog
    if (!confirm('Are you sure you want to cancel this restaurant booking?')) {
        console.log("Restaurant booking cancellation cancelled by user.");
        return; // Stop if user cancels
    }

    const url = `/api/Bookings/${bookingId}`; // Assuming your Restaurant Booking API endpoint is /api/Bookings/{id}

    try {
        // Use authFetch to make the authenticated DELETE request
        const response = await authFetch(url, {
            method: 'DELETE' // Or POST if your API uses a different method for cancellation
            // If your API expects a body for cancellation, add headers and body here
        });

        if (response.ok) {
            console.log("Restaurant booking cancelled successfully!");
            // Display a success message or refresh the page
            // A simple page reload will re-fetch the updated list
            window.location.reload();
        } else {
            const errorData = await response.json();
            console.error("Failed to cancel restaurant booking:", response.status, errorData);
            // Display an error message to the user (e.g., using an alert or a dedicated message area)
            alert("Failed to cancel restaurant booking: " + (errorData.message || "Unknown error."));
        }

    } catch (error) {
        console.error("Error during restaurant booking cancellation fetch:", error);
        // authFetch handles token missing/expiration by redirecting to login
        // Other errors will be caught here
        if (error.message !== "Authentication token not found." && error.message !== "Authentication failed. Please log in again.") {
             alert("An error occurred during cancellation: " + error.message);
        }
    }
}


/**
 * Handles the click event for editing a Consultant booking.
 * Redirects to the MyBookings page with the consultant booking ID in the query string.
 * @param {number} bookingId The integer ID of the consultant booking to edit.
 */
function handleEditConsultantBooking(bookingId) {
    console.log(`Initiating edit for Consultant Booking ID: ${bookingId}`);
    // Redirect to the current page with the editConsultantBookingId query parameter
    window.location.href = `/Book/MyBookings?editConsultantBookingId=${bookingId}`;
}

/**
 * Handles the click event for canceling a Consultant booking.
 * Makes an authenticated API call to cancel the booking.
 * @param {number} bookingId The integer ID of the consultant booking to cancel.
 */
async function handleCancelConsultantBooking(bookingId) {
    console.log(`Attempting to cancel Consultant Booking ID: ${bookingId}`);

    // Optional: Show a confirmation dialog
    if (!confirm('Are you sure you want to cancel this consultant booking?')) {
        console.log("Consultant booking cancellation cancelled by user.");
        return; // Stop if user cancels
    }

    // Assuming your Consultant Booking API endpoint is /api/ConsultantBookings/{id}
    const url = `/api/ConsultantBookings/${bookingId}`;

    try {
        // Use authFetch to make the authenticated DELETE request
        const response = await authFetch(url, {
            method: 'DELETE' // Or POST if your API uses a different method for cancellation
            // If your API expects a body for cancellation, add headers and body here
        });

        if (response.ok) {
            console.log("Consultant booking cancelled successfully!");
            // Display a success message or refresh the page
            // A simple page reload will re-fetch the updated list
            window.location.reload();
        } else {
            const errorData = await response.json();
            console.error("Failed to cancel consultant booking:", response.status, errorData);
            // Display an error message to the user
            alert("Failed to cancel consultant booking: " + (errorData.message || "Unknown error."));
        }

    } catch (error) {
        console.error("Error during consultant booking cancellation fetch:", error);
        // authFetch handles token missing/expiration by redirecting to login
        // Other errors will be caught here
        if (error.message !== "Authentication token not found." && error.message !== "Authentication failed. Please log in again.") {
             alert("An error occurred during cancellation: " + error.message);
        }
    }
}

// Make all relevant functions available globally
window.authFetch = authFetch;
window.handleLogin = handleLogin;
window.handleRegister = handleRegister;
window.handleForgotPassword = handleForgotPassword;
window.handleResetPassword = handleResetPassword;
window.handleEditRestaurantBooking = handleEditRestaurantBooking;
window.handleCancelRestaurantBooking = handleCancelRestaurantBooking;
window.handleEditConsultantBooking = handleEditConsultantBooking;
window.handleCancelConsultantBooking = handleCancelConsultantBooking;

// If you are using ES modules (requires module type script tags), you would export:
// export { authFetch, handleLogin, handleRegister, handleForgotPassword, handleResetPassword, handleEditRestaurantBooking, handleCancelRestaurantBooking, handleEditConsultantBooking, handleCancelConsultantBooking };
