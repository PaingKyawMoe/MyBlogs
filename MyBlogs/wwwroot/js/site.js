

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
