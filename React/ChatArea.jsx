// @flow
import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Link } from 'react-router-dom';
import { Card, Dropdown, Row, Col } from 'react-bootstrap';
import classnames from 'classnames';
import SimpleBar from 'simplebar-react';
import PropTypes from 'prop-types';
import ChatMessageForm from './ChatMessageForm';
import { getMessagesAll, postMessage } from '../../services/messageService';
import * as moment from 'moment';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { onGlobalSuccess, onGlobalError } from '../../services/serviceHelpers';
import debug from 'sabio-debug';
import avatar11 from '../../assets/images/users/avatar-11.jpg';
import '../../assets/scss/Saas.scss';
import '../../assets/scss/custom/fonts/_nunito.scss';
import Loader from '../Loader';

const _logger = debug.extend('ChatArea');

const UserMessage = ({ message, toUser }) => {
    return (
        <li className={classnames('clearfix', { odd: message.sender.id === toUser.id })}>
            <div className="chat-avatar">
                <img src={message.sender.avatarUrl ? message.sender.avatarUrl : avatar11} className="rounded" alt="" />
                <i>{moment(message.dateSent).format('HH:mm')}</i>
            </div>

            <div className="conversation-text">
                <div className="ctext-wrap">
                    <i>{message.sender.firstName}</i>
                    <p>{message.messageContent}</p>
                </div>
                {message.messageContent.type === 'file' && (
                    <Card className="mt-2 mb-1 shadow-none border text-start">
                        <div className="p-2">
                            <Row className="align-items-center">
                                <Col className="col-auto">
                                    <div className="avatar-sm">
                                        <span className="avatar-title rounded">
                                            <i className="uil uil-file-upload-alt font-20"></i>
                                        </span>
                                    </div>
                                </Col>
                                <Col className="ps-0">
                                    {/* <Link to="#" className="text-muted fw-bold">
                                        {message.message.value.file}
                                    </Link> */}
                                    {/* <p className="mb-0">{message.message.value.size}</p> */}
                                </Col>
                                <Col className="col-auto">
                                    <Link to="#" className="btn btn-link btn-lg text-muted">
                                        <i className="dripicons-download"></i>
                                    </Link>
                                </Col>
                            </Row>
                        </div>
                    </Card>
                )}
            </div>

            <Dropdown className="conversation-actions" align="end">
                <Dropdown.Toggle variant="link" className="btn btn-sm btn-link arrow-none shadow-none">
                    <i className="uil uil-ellipsis-v"></i>
                </Dropdown.Toggle>
                <Dropdown.Menu>
                    <Dropdown.Item>Copy Message</Dropdown.Item>
                    <Dropdown.Item>Edit</Dropdown.Item>
                    <Dropdown.Item>Delete</Dropdown.Item>
                </Dropdown.Menu>
            </Dropdown>
        </li>
    );
};

// ChatArea
const ChatArea = (props) => {
    const selectedUser = props.user;
    const toUser = props.activeUser;

    const [loading, setLoading] = useState(true);
    const [userMessages, setUserMessages] = useState([]);
    const latestChat = useRef(null);

    latestChat.current = userMessages;

    /*
     *  Fetches the messages for selected user
     */
    const getMessagesForUser = useCallback(() => {
        _logger('getMessages firing');
        if (selectedUser) {
            setLoading(true);
            setTimeout(() => {
                getMessagesAll(0, 500).then(onGetMessagesAllSuccess).catch(onGlobalError);
                setLoading(false);
            }, 750);
        }
    }, [selectedUser, toUser]);

    useEffect(() => {
        getMessagesForUser();
    }, [getMessagesForUser]);

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl('https://localhost:50001/chathub')
            .withAutomaticReconnect()
            .build();

        connection
            .start()
            .then((result) => {
                connection.on('ReceiveMessage', (m) => {
                    const updatedChat = [...latestChat.current];
                    if (m.sender.id === toUser.id || m.recipient.id === toUser.id) {
                        _logger('new message sent in this conversation');
                        updatedChat.push(m);
                    }
                    setUserMessages(updatedChat);
                });

                _logger('Connection Successful', result);
            })

            .catch(_logger('Connection Failed'));
    }, []);

    const onGetMessagesAllSuccess = (response) => {
        _logger('onGetMessagesAllSuccess');
        const arrayOfRecievedMessages = response.data.item.pagedItems;

        setUserMessages(
            arrayOfRecievedMessages.filter(
                (m) =>
                    (m.recipient.id === toUser.id && m.sender.id === selectedUser.id) ||
                    (toUser.id === m.sender.id && m.recipient.id === selectedUser.id)
            )
        );
    };

    /**
     * sends the chat message
     */
    const sendChatMessage = (message) => {
        let newUserMessage = {
            message: message.newMessage,
            subject: '',
            recipientId: selectedUser.id,
            senderId: toUser.id,
            dateSent: moment().format('YYYY-MM-DDTHH:mm'),
            dateRead: null,
        };

        postMessage(newUserMessage).then(onGlobalSuccess).catch(onGlobalError);
    };

    return (
        <>
            <Card>
                <Card.Body className="position-relative px-0 pb-0">
                    {loading && <Loader />}

                    <SimpleBar style={{ height: '538px', width: '100%' }}>
                        <ul className="conversation-list px-3">
                            {userMessages.map((message, index) => {
                                return <UserMessage key={index} message={message} toUser={toUser} />;
                            })}
                        </ul>
                    </SimpleBar>

                    <ChatMessageForm onSendClicked={sendChatMessage} />
                </Card.Body>
            </Card>
        </>
    );
};

ChatArea.propTypes = {
    user: PropTypes.shape({
        id: PropTypes.number.isRequired,
        firstName: PropTypes.string.isRequired,
        lastName: PropTypes.string.isRequired,
        avatar: PropTypes.string,
        lastMessage: PropTypes.string,
        lastMessageOn: PropTypes.string,
    }),
    activeUser: PropTypes.shape({
        email: PropTypes.string.isRequired,
        id: PropTypes.number.isRequired,
        isLoggedIn: PropTypes.bool.isRequired,
        profilePic: PropTypes.string.isRequired,
        roles: PropTypes.arrayOf(PropTypes.string.isRequired),
    }),
};

UserMessage.propTypes = {
    message: PropTypes.shape({
        id: PropTypes.number.isRequired,
        messageContent: PropTypes.string.isRequired,
        recipient: PropTypes.shape({
            id: PropTypes.number.isRequired,
            firstName: PropTypes.string,
            lastName: PropTypes.string,
            avatarUrl: PropTypes.string,
        }),
        sender: PropTypes.shape({
            id: PropTypes.number.isRequired,
            firstName: PropTypes.string,
            lastName: PropTypes.string,
            avatarUrl: PropTypes.string,
        }),
        dateSent: PropTypes.string.isRequired,
    }),
    toUser: PropTypes.shape({
        email: PropTypes.string.isRequired,
        id: PropTypes.number.isRequired,
        isLoggedIn: PropTypes.bool.isRequired,
        profilePic: PropTypes.string.isRequired,
        roles: PropTypes.arrayOf(PropTypes.string.isRequired),
    }),
};

export default ChatArea;
