interface DeliveryLetter {
    id: string;
    sender: string;
    receiverType: string;
    receiver: string;
    subject: string;
    content: string;
    comeFrom?: string;
    sentOn: Date;
    operator: string;
}