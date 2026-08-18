import api from './api';

export const apiGet = async () => {
    return (await api.get("/users")).data;
}