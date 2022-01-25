var LoginFun = {
	vars: {
		login: document.querySelector('.login'),
		login_brand: document.querySelector('.login-brand'),
		login_wrapper: document.querySelector('.login-wrapper'),
		login_login: document.querySelector('.login-login'),
		login_wrapper_height: 0,
		login_back_link: document.querySelector('.login-back-link'),
		forgot_link: document.querySelector('.forgot-link'),
		login_link: document.querySelector('.login-link'),
		login_btn: document.querySelector('.login-btn'),
		register_link: document.querySelector('.register-link'),
		password_group: document.querySelector('.password-group'),
		password_group_height: 0,
		login_register: document.querySelector('.login-register'),
		login_footer: document.querySelector('.login-footer'),
		box: document.getElementsByClassName('login-box'),
		option: {}
	},
	register(e) {
		LoginFun.vars.login_login.className += ' login-animated';
		setTimeout(() => {
			LoginFun.vars.login_login.style.display = 'none';
		}, 500);
		LoginFun.vars.login_register.style.display = 'block';
		LoginFun.vars.login_register.className += ' login-animated-flip';

		LoginFun.setHeight(LoginFun.vars.login_register.offsetHeight + LoginFun.vars.login_footer.offsetHeight);

		e.preventDefault();
	},
	login(e) {
		LoginFun.vars.login_register.classList.remove('login-animated-flip');
		LoginFun.vars.login_register.className += ' login-animated-flipback';
		LoginFun.vars.login_login.style.display = 'block';
		LoginFun.vars.login_login.classList.remove('login-animated');
		LoginFun.vars.login_login.className += ' login-animatedback';
		setTimeout(() => {
			LoginFun.vars.login_register.style.display = 'none';
		}, 500);

		setTimeout(() => {
			LoginFun.vars.login_register.classList.remove('login-animated-flipback');
			LoginFun.vars.login_login.classList.remove('login-animatedback');
		}, 1000);

		LoginFun.setHeight(LoginFun.vars.login_login.offsetHeight + LoginFun.vars.login_footer.offsetHeight);

		e.preventDefault();
	},
	forgot(e) {
		LoginFun.vars.password_group.classList += ' login-animated';
		LoginFun.vars.login_back_link.style.display = 'block';

		setTimeout(() => {
			LoginFun.vars.login_back_link.style.opacity = 1;
			LoginFun.vars.password_group.style.height = 0;
			LoginFun.vars.password_group.style.margin = 0;
		}, 100);

		LoginFun.vars.login_btn.innerText = 'Forgot Password';

		LoginFun.setHeight(LoginFun.vars.login_wrapper_height - LoginFun.vars.password_group_height);
		LoginFun.vars.login_login.querySelector('form').setAttribute('action', LoginFun.vars.option.forgot_url);

		e.preventDefault();
	},
	loginback(e) {
		LoginFun.vars.password_group.classList.remove('login-animated');
		LoginFun.vars.password_group.classList += ' login-animated-back';
		LoginFun.vars.password_group.style.display = 'block';

		setTimeout(() => {
			LoginFun.vars.login_back_link.style.opacity = 0;
			LoginFun.vars.password_group.style.height = LoginFun.vars.password_group_height + 'px';
			LoginFun.vars.password_group.style.marginBottom = 30 + 'px';
		}, 100);

		setTimeout(() => {
			LoginFun.vars.login_back_link.style.display = 'none';
			LoginFun.vars.password_group.classList.remove('login-animated-back');
		}, 1000);

		LoginFun.vars.login_btn.innerText = 'Sign In';
		LoginFun.vars.login_login.querySelector('form').setAttribute('action', LoginFun.vars.option.login_url);

		LoginFun.setHeight(LoginFun.vars.login_wrapper_height);

		e.preventDefault();
	},
	setHeight(height) {
		LoginFun.vars.login_wrapper.style.minHeight = height + 'px';
	},
	brand() {
		LoginFun.vars.login_brand.classList += ' login-animated';
		setTimeout(() => {
			LoginFun.vars.login_brand.classList.remove('login-animated');
		}, 1000);
	},
	init(option) {
		LoginFun.setHeight(LoginFun.vars.box[0].offsetHeight + LoginFun.vars.login_footer.offsetHeight);

		LoginFun.vars.password_group.style.height = LoginFun.vars.password_group.offsetHeight + 'px';
		LoginFun.vars.password_group_height = LoginFun.vars.password_group.offsetHeight;
		LoginFun.vars.login_wrapper_height = LoginFun.vars.login_wrapper.offsetHeight;

		LoginFun.vars.option = option;
		LoginFun.vars.login_login.querySelector('form').setAttribute('action', option.login_url);

		var len = LoginFun.vars.box.length - 1;

		for (var i = 0; i <= len; i++) {
			if (i !== 0) {
				LoginFun.vars.box[i].className += ' login-flip';
			}
		}

		LoginFun.vars.forgot_link.addEventListener("click", (e) => {
			LoginFun.forgot(e);
		});

		LoginFun.vars.register_link.addEventListener("click", (e) => {
			LoginFun.brand();
			LoginFun.register(e);
		});

		LoginFun.vars.login_link.addEventListener("click", (e) => {
			LoginFun.brand();
			LoginFun.login(e);
		});

		LoginFun.vars.login_back_link.addEventListener("click", (e) => {
			LoginFun.loginback(e);
		});
	}
}