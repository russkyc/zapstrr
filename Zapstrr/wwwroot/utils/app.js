const appRouter = {
    authenticated: false,
    username: '',
    nightMode: true,
    init(){
        let token = window.sessionStorage.getItem('token');
        if (token !== null){
            let user = parseJwt(token);
            this.username = user.username;
            this.authenticated = true;
        }
    },
    toggleNightMode(){
        this.nightMode = !this.nightMode;
    },
    logout(){
        window.sessionStorage.removeItem('token');
        this.authenticated = false;
        this.$router.navigate('/login');
    },
    async authorize(context) {
        let token = window.sessionStorage.getItem('token');
        if (!token) {
            if (context.route !== '/login' && context.route !== '/register') {
                return context.redirect('/login');
            }
        } else {
            try {
                let claims = parseJwt(token);
                if (context.route === '/login' || context.route === '/register') {
                    return context.redirect('/quizzes');
                }
            } catch (e) {
                if (context.route !== '/login') {
                    return context.redirect('/login');
                }
            }
        }
    }
};