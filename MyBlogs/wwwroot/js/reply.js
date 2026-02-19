$(document).ready(function () {
    // 1. Handle Reply Button Click
    $(document).on('click', '.reply-btn', function () {
        var id = $(this).data('id');
        var user = $(this).data('user');
        $('#parentId').val(id);
        $('#replyIndicator').removeClass('d-none');
        $('#replyingToUser').text(user);
        $('html, body').animate({ scrollTop: $("#commentForm").offset().top - 150 }, 500);
        $('#Content').focus();
    });

    // 2. Handle Form Submit
    $("#commentForm").on('submit', function (event) {
        event.preventDefault();

        // Get values from the form attributes we set in step 1
        var postId = $(this).data('post-id');
        var actionUrl = $(this).data('url');

        var requestData = {
            Username: $("#userName").val(),
            Content: $("#Content").val(),
            PostId: postId,
            ParentId: $('#parentId').val() ? parseInt($('#parentId').val()) : null
        };

        $.ajax({
            url: actionUrl,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(requestData),
            success: function () {
                location.reload();
            }
        });
    });
});