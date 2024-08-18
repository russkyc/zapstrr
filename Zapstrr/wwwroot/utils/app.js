const app = {
    nightMode: true,
    toggleNightMode(){
        this.nightMode = !this.nightMode;
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
                if (context.route === '/login') {
                    return context.redirect('/');
                }
            } catch (e) {
                if (context.route !== '/login') {
                    return context.redirect('/login');
                }
            }
        }
    }
};