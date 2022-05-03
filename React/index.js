import React, { useState, useEffect } from 'react';
import { getMessagesByRecipientId} from '../../services/messageService';
import * as moment from 'moment';
import PropTypes from 'prop-types';
import ChatUsers from './ChatUsers';
import ChatArea from './ChatArea';
import { Row, Col } from 'react-bootstrap';
import { onGlobalError } from '../../services/serviceHelpers';
import debug from 'sabio-debug';
import '../../assets/scss/Saas.scss';
import '../../assets/scss/custom/fonts/_nunito.scss';  

const _logger = debug.extend('ChatApp');
// ChatApp
const ChatApp = (props) => {
    const currentUser = props.currentUser;

    const [user, setUser] = useState([]);
    const [selectedUser, setSelectedUser] = useState(user[0]);         

    useEffect(() => {          
        _logger('useEffect firing');
        getMessagesByRecipientId(currentUser.id, 0, 10).then(onGetMessagesByRecipientIdSuccess).catch(onGlobalError);
    }, [currentUser]);        

    const onGetMessagesByRecipientIdSuccess = (response) => {    
        _logger('onGetMessagesByRecipientIdSuccess', response);                                           
        const arrayOfMessages = response.data.item.pagedItems;       
        const mappedUsers = arrayOfMessages.map((m) => {      
            let user = {
                id: m.sender.id,
                firstName: m.sender.firstName,
                lastName: m.sender.lastName,
                avatar: m.sender.avatarUrl,
                lastMessage: m.messageContent,
                lastMessageOn: moment(m.dateSent).format("MMM Do YY"),            
            };
            return user;
        });

        const sortedUsers = mappedUsers.sort((a, b) => {
            return a.lastMessageOn.localeCompare(b.lastMessageOn);          
        });

        const uniqueUsers = [];

        setUser(
            sortedUsers.filter((u) => {
                let result = false;
                if (!uniqueUsers.includes(u.id)) {
                    uniqueUsers.push(u.id);

                    result = true;
                }
                return result;
            })
        );
    };

    const onUserChange = (user) => {
        _logger('User Selected', user);
        setSelectedUser(user);
    };

    return (        
        <>


            <Row>
                <Col xxl={6} xl={{ span: 6, order: 1 }}>    
                    <ChatUsers activeUser = {currentUser} onUserSelect = {onUserChange} users = {user} />
                </Col>                                                                        

                <Col xxl={6} xl={{ span: 6, order: 2 }}>        
                    <ChatArea user={selectedUser} activeUser={currentUser} />
                </Col> 
            </Row>

        </>
    );
};

ChatApp.propTypes = {
    currentUser: PropTypes.shape({
        email: PropTypes.string.isRequired,
        id: PropTypes.number.isRequired,
        isLoggedIn: PropTypes.bool.isRequired,
        profilePic: PropTypes.string.isRequired,
        roles: PropTypes.arrayOf(PropTypes.string),
    }),  
};

export default ChatApp;                                                                   
