interface DeliveryLetterListItem {
    id: string;
    sender: string;
    receiverType: string;
    receiver?: {};
    subject: string;
    sentOn: Date;
}