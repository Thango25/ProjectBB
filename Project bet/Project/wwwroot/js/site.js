$(document).ready(function () {
    // --- Common Functions (Existing or new) ---

    // Example for your existing "Upload Lost Item Modal" (if it uses similar logic)
    // You might already have some JS here, if so, integrate this new part.
    // Assuming your "Upload Lost Item Modal" is static for now.

    // --- Profile Modal Specific Functions ---
    const profileModal = $('#profileModal');
    const profileNavLink = $('#profileNavLink');
    const profileModalBodyContent = $('#profileModalBodyContent');
    const editProfileBtn = $('#editProfileBtn');
    const saveProfileBtn = $('#saveProfileBtn');

    // Function to load profile content into the modal body via AJAX
    function loadProfileContent() {
        $.get('/Profile/GetProfileData', function (data) {
            profileModalBodyContent.html(data);
            // Re-initialize client-side validation for the new form
            var form = $('#profileForm');
            form.removeData('validator');
            form.removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);

            // After content is loaded, set the initial mode to display
            toggleProfileMode(false);
        }).fail(function () {
            profileModalBodyContent.html('<div class="text-danger">Failed to load profile. Please try again.</div>');
        });
    }

    // Function to toggle between display and edit modes
    function toggleProfileMode(isEditing) {
        // These elements are inside the dynamically loaded partial view,
        // so we select them within the modal's content
        const profileDisplay = profileModalBodyContent.find('#profileDisplay');
        const profileEdit = profileModalBodyContent.find('#profileEdit');
        const userProfileAvatar = profileModalBodyContent.find('#userProfileAvatar');
        const avatarUpload = profileModalBodyContent.find('#avatarUpload');

        if (isEditing) {
            profileDisplay.hide();
            profileEdit.show();
            editProfileBtn.hide();
            saveProfileBtn.show();

            // Populate edit fields with current display values
            profileModalBodyContent.find('#Username').val(profileModalBodyContent.find('#displayUsername').text());
            profileModalBodyContent.find('#Email').val(profileModalBodyContent.find('#displayEmail').text());
            profileModalBodyContent.find('#PhoneNumber').val(profileModalBodyContent.find('#displayPhone').text());
            profileModalBodyContent.find('#Location').val(profileModalBodyContent.find('#displayLocation').text());

        } else {
            profileDisplay.show();
            profileEdit.hide();
            editProfileBtn.show();
            saveProfileBtn.hide();
        }

        // Attach avatar upload preview listener (needs to be re-attached if content is dynamic)
        avatarUpload.off('change').on('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    userProfileAvatar.attr('src', e.target.result);
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // --- Event Handlers ---

    // When the "Profile" nav link is clicked, load the content and then show the modal
    profileNavLink.on('click', function (e) {
        e.preventDefault(); // Prevent default link behavior
        loadProfileContent(); // Load content first
        profileModal.modal('show'); // Then show the modal
    });

    // Handle Edit button click
    editProfileBtn.on('click', function () {
        toggleProfileMode(true);
    });

    // Handle Save button click
    saveProfileBtn.on('click', function (e) {
        e.preventDefault(); // Prevent default button behavior
        var form = profileModalBodyContent.find('#profileForm'); // Select the form inside the modal

        // Client-side validation check
        if (!form.valid()) {
            return; // Stop if form is not valid
        }

        var formData = new FormData(form[0]); // Create FormData object for file upload

        // Append avatar file if selected
        var avatarFile = profileModalBodyContent.find('#avatarUpload')[0].files[0];
        if (avatarFile) {
            formData.append('AvatarFile', avatarFile);
        }

        $.ajax({
            url: '/Profile/SaveProfile', // MVC action URL
            type: 'POST',
            data: formData,
            processData: false, // Important for FormData
            contentType: false, // Important for FormData
            success: function (response) {
                if (response.success) {
                    alert(response.message); // Show success message

                    // Update the display values from the response
                    profileModalBodyContent.find('#displayUsername').text(response.profile.username);
                    profileModalBodyContent.find('#displayEmail').text(response.profile.email);
                    profileModalBodyContent.find('#displayPhone').text(response.profile.phoneNumber);
                    profileModalBodyContent.find('#displayLocation').text(response.profile.location);
                    if (response.profile.avatarUrl) {
                        profileModalBodyContent.find('#userProfileAvatar').attr('src', response.profile.avatarUrl);
                    }

                    toggleProfileMode(false); // Switch back to display mode
                } else {
                    alert('Error: ' + response.message + '\n' + (response.errors ? response.errors.join('\n') : ''));
                }
            },
            error: function (xhr, status, error) {
                alert('An error occurred while saving profile: ' + xhr.responseText);
            }
        });
    });
});