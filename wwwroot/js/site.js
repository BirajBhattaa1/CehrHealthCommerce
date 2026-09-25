(function () {
    "use strict";

    const validationRules = {
        nepaliPhone: /^(?:\+977[- ]?)?9[78]\d{8}$/,
        email: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
        postalCode: /^[0-9]{5}$/,
        nid: /^\d{10,16}$/
    };

    function validatePhoneNumber(input) {
        const value = input.value.trim();
        const errorElement = input.parentElement.querySelector('.form-validation-error') || createErrorElement(input);

        if (!value) {
            showError(input, errorElement, 'Phone number is required');
            return false;
        }

        if (!validationRules.nepaliPhone.test(value)) {
            showError(input, errorElement, 'Enter a valid 10-digit Nepali mobile number starting with 97 or 98');
            return false;
        }

        clearError(input, errorElement);
        return true;
    }

    function validateEmailAddress(input) {
        const value = input.value.trim();
        const errorElement = input.parentElement.querySelector('.form-validation-error') || createErrorElement(input);

        if (!value) {
            showError(input, errorElement, 'Email address is required');
            return false;
        }

        if (!validationRules.email.test(value)) {
            showError(input, errorElement, 'Enter a valid email address');
            return false;
        }

        clearError(input, errorElement);
        return true;
    }

    function validatePostalCode(input) {
        const value = input.value.trim();
        if (!value) return true;

        const errorElement = input.parentElement.querySelector('.form-validation-error') || createErrorElement(input);

        if (!/^[0-9]{5}$/.test(value)) {
            showError(input, errorElement, 'Postal code must be 5 digits');
            return false;
        }

        clearError(input, errorElement);
        return true;
    }

    function validateNID(input) {
        const value = input.value.replace(/\D/g, '');
        const errorElement = input.parentElement.querySelector('.form-validation-error') || createErrorElement(input);

        if (!value) {
            showError(input, errorElement, 'National ID is required');
            return false;
        }

        if (!validationRules.nid.test(value)) {
            showError(input, errorElement, 'NID must be 10 to 16 digits');
            return false;
        }

        clearError(input, errorElement);
        return true;
    }

    function showError(input, errorElement, message) {
        input.classList.add('input-error');
        input.classList.remove('input-success');
        errorElement.textContent = message;
        errorElement.style.display = 'block';
    }

    function clearError(input, errorElement) {
        input.classList.remove('input-error');
        input.classList.add('input-success');
        errorElement.textContent = '';
        errorElement.style.display = 'none';
    }

    function createErrorElement(input) {
        const error = document.createElement('span');
        error.className = 'form-validation-error';
        error.style.display = 'none';
        input.parentElement.appendChild(error);
        return error;
    }

    function attachValidationListeners() {
        const phoneInputs = document.querySelectorAll('input[type="tel"], input[name*="Phone"], input[id*="Phone"]');
        phoneInputs.forEach(input => {
            input.addEventListener('blur', () => validatePhoneNumber(input));
            input.addEventListener('input', () => {
                if (input.value.length >= 10) {
                    validatePhoneNumber(input);
                }
            });
        });

        const emailInputs = document.querySelectorAll('input[type="email"], input[name*="Email"], input[id*="Email"]');
        emailInputs.forEach(input => {
            input.addEventListener('blur', () => validateEmailAddress(input));
        });

        const postalInputs = document.querySelectorAll('input[name*="PostalCode"], input[id*="PostalCode"]');
        postalInputs.forEach(input => {
            input.addEventListener('blur', () => validatePostalCode(input));
        });

        const nidInputs = document.querySelectorAll('input[name*="NID"], input[id*="NID"]');
        nidInputs.forEach(input => {
            input.addEventListener('blur', () => validateNID(input));
        });
    }

    function preventDoubleSubmit() {
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', function(e) {
                const submitButton = form.querySelector('button[type="submit"]');
                if (submitButton && !submitButton.disabled) {
                    submitButton.disabled = true;
                    const originalText = submitButton.innerHTML;
                    submitButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Processing...';

                    setTimeout(() => {
                        if (submitButton.disabled) {
                            submitButton.disabled = false;
                            submitButton.innerHTML = originalText;
                        }
                    }, 3000);
                }
            });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
        tooltips.forEach(el => {
            new bootstrap.Tooltip(el);
        });

        attachValidationListeners();
        preventDoubleSubmit();
    });
})();
