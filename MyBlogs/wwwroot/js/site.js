
// --- 1. LIKE BUTTON LOGIC ---
$(document).ready(function () {
    $(".like-btn").click(function () {
        var postId = $(this).data("id");
        var button = $(this);

        $.ajax({
            url: '/Post/LikePost', // Make sure this matches your Controller/Action
            type: 'POST',
            data: { id: postId },
            success: function (response) {
                // This updates the number inside the span
                $(".like-count-" + postId).text(response.count);

                // Optional: make the heart solid
                button.find("i").removeClass("bi-heart").addClass("bi-heart-fill");
                button.addClass("btn-danger text-white").removeClass("btn-outline-danger");
            },
            error: function () {
                alert("Error liking post.");
            }
        });
    });
});




$(document).ready(function () {
    // Universal Password Toggle for both Login and Register
    $(document).on('click', '.toggle-password-btn', function () {
        // Find the input field immediately before this button
        const input = $(this).siblings('input');
        const icon = $(this).find('i');

        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('bi-eye').addClass('bi-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('bi-eye-slash').addClass('bi-eye');
        }
    });
});