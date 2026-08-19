export const auth = {

    getToken: () => {
        return localStorage.getItem("accessToken");
    },

    isAuthenticated: () => {
        return !!localStorage.getItem("accessToken");
    },

    logout: () => {
        localStorage.removeItem("accessToken");
    }

};