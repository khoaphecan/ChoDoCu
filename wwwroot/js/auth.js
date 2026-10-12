const messageBox =
    document.getElementById("message");


function showMessage(message, type) {

    messageBox.textContent = message;

    messageBox.className =
        "message " + type;
}


/*
====================================
ĐĂNG NHẬP
====================================
*/

const loginForm =
    document.getElementById("loginForm");


if (loginForm) {

    loginForm.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();

            const email =
                document.getElementById("email").value;

            const password =
                document.getElementById("password").value;


            try {

                const response =
                    await fetch(
                        "/api/login",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body: JSON.stringify({
                                email: email,
                                password: password
                            })
                        }
                    );


                const data =
                    await response.json();


                if (!response.ok) {

                    showMessage(
                        data.message,
                        "error"
                    );

                    return;
                }


                showMessage(
                    "Đăng nhập thành công!",
                    "success"
                );


                setTimeout(function () {

                    document.body.classList.remove("page-loaded");
                    document.body.classList.add("page-exit");

                    setTimeout(function () {

                        window.location.href = "index.html";

                    }, 350);

                }, 700);

            }

            catch (error) {

                showMessage(
                    "Không thể kết nối máy chủ.",
                    "error"
                );

            }

        }
    );
}


/*
====================================
ĐĂNG KÝ
====================================
*/

const registerForm =
    document.getElementById(
        "registerForm"
    );


if (registerForm) {

    registerForm.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const fullName =
                document.getElementById(
                    "fullName"
                ).value;

            const email =
                document.getElementById(
                    "email"
                ).value;

            const phone =
                document.getElementById(
                    "phone"
                ).value;

            const password =
                document.getElementById(
                    "password"
                ).value;

            const confirmPassword =
                document.getElementById(
                    "confirmPassword"
                ).value;


            if (password !== confirmPassword) {

                showMessage(
                    "Mật khẩu nhập lại không khớp.",
                    "error"
                );

                return;
            }


            try {

                const response =
                    await fetch(
                        "/api/register",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body: JSON.stringify({

                                fullName:
                                    fullName,

                                email:
                                    email,

                                phone:
                                    phone,

                                password:
                                    password,

                                confirmPassword:
                                    confirmPassword

                            })
                        }
                    );


                const data =
                    await response.json();


                if (!response.ok) {

                    showMessage(
                        data.message,
                        "error"
                    );

                    return;
                }


                showMessage(
                    "Đăng ký thành công!",
                    "success"
                );


                setTimeout(function () {

                    document.body.classList.remove("page-loaded");
                    document.body.classList.add("page-exit");

                    setTimeout(function () {

                        window.location.href = "index.html";

                    }, 350);

                }, 700);

            }

            catch (error) {

                showMessage(
                    "Không thể kết nối máy chủ.",
                    "error"
                );

            }

        }
    );
}