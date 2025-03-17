export interface Profile {
    id: string;
    name?: string;
    surname?: string;
    email?: string;
    phoneNumber?: string;
    dateBirth?: Date;
    description: string;
    imageUrl?: string;
    positionTitle?: string;
    isActive: boolean;
}