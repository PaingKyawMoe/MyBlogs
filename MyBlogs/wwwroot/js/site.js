
// --- 1. LIKE BUTTON LOGIC ---
$(".like-btn").click(function () {
    var button = $(this);
    var postId = button.data("id");
    var url = button.data("url"); // Get the URL from the button
    var token = $('meta[name="csrf-token"]').attr("content");

    $.ajax({
        url: url,
        type: 'POST',
        headers: { "RequestVerificationToken": token },
        data: { id: postId },
        success: function (response) {
            $(".like-count-" + postId).text(response.count);
            button.find("i").removeClass("bi-heart").addClass("bi-heart-fill");
            button.addClass("btn-danger text-white").removeClass("btn-outline-danger");
        },
        error: function (xhr) {
            console.log("Error:", xhr.statusText);
        }
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